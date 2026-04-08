using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

public class GithubService(IConfigService config) : IGithubService
{
    private readonly ConcurrentDictionary<string, List<string>> _assigneeCache = new();
    private readonly IConfigService _config = config;
    private readonly ConcurrentDictionary<string, List<string>> _labelCache = new();
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _prStatusCache = new();
    private List<RepoConfig>? _repoCache;

    public List<RepoConfig> GetRepos()
    {
        if (_repoCache is not null)
            return _repoCache;

        var uniquePaths = _config.Settings.Projects
            .SelectMany(p => p.RepoPaths)
            .Distinct()
            .ToList();

        var repos = new List<RepoConfig>();
        foreach (var repoPath in uniquePaths)
        {
            var repoConfig = GetRepoConfigFromPath(repoPath);
            if (repoConfig is not null)
                repos.Add(repoConfig);
        }

        _repoCache = repos;
        return repos;
    }

    public async Task<List<string>> GetAssigneesAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_assigneeCache.TryGetValue(key, out var cached))
            return cached;

        var assignees = await FetchAssigneesFromGhCliAsync(owner, repo);
        _assigneeCache[key] = assignees;
        return assignees;
    }

    public async Task<List<string>> GetLabelsAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_labelCache.TryGetValue(key, out var cached))
            return cached;

        var labels = await FetchLabelsFromGhCliAsync(owner, repo);
        _labelCache[key] = labels;
        return labels;
    }

    public async Task<Dictionary<string, string>> GetPrStatusesAsync(string owner, string repo)
    {
        var key = $"{owner}/{repo}";
        if (_prStatusCache.TryGetValue(key, out var cached))
            return cached;

        var statuses = await FetchPrStatusesFromGhCliAsync(owner, repo);
        _prStatusCache[key] = statuses;
        return statuses;
    }

    internal static RepoConfig? GetRepoConfigFromPath(string repoPath)
    {
        try
        {
            var psi = new ProcessStartInfo("git", "remote get-url origin")
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null) return null;

            var url = process.StandardOutput.ReadToEnd().Trim();
            if (!process.WaitForExit(10000))
            {
                try { process.Kill(true); } catch { /* already exited */ }
            }
            if (process.ExitCode != 0) return null;

            return ParseRepoConfigFromUrl(url);
        }
        catch
        {
            return null;
        }
    }

    internal static RepoConfig? ParseRepoConfigFromUrl(string url)
    {
        var match = Regex.Match(url, @"[/:](?<owner>[^/]+)/(?<name>[^/]+?)(?:\.git)?$");
        if (!match.Success) return null;

        return new RepoConfig
        {
            Owner = match.Groups["owner"].Value,
            Name = match.Groups["name"].Value
        };
    }

    private static async Task<List<string>> FetchLabelsFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/labels --jq \".[].name\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null) return new List<string>();

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            using var ctsLabels = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                await process.WaitForExitAsync(ctsLabels.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { /* already exited */ }
            }

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api labels failed for {owner}/{repo}: {stderr}");
                return new List<string>();
            }

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch labels for {owner}/{repo}: {ex.Message}");
            return new List<string>();
        }
    }

    private static async Task<Dictionary<string, string>> FetchPrStatusesFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh",
                $"pr list --repo {owner}/{repo} --limit 100 --state all --json url,state")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            using var ctsPrList = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                await process.WaitForExitAsync(ctsPrList.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { /* already exited */ }
            }

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine(
                    $"[GithubService] gh pr list failed for {owner}/{repo}: {stderr}");
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var doc = JsonDocument.Parse(output);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var url = element.GetProperty("url").GetString();
                var state = element.GetProperty("state").GetString();
                if (url is not null && state is not null)
                {
                    var titleCase = state switch
                    {
                        "OPEN" => "Open",
                        "CLOSED" => "Closed",
                        "MERGED" => "Merged",
                        _ => state
                    };
                    result[url] = titleCase;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[GithubService] Failed to fetch PR statuses for {owner}/{repo}: {ex.Message}");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static async Task<List<string>> FetchAssigneesFromGhCliAsync(string owner, string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("gh", $"api repos/{owner}/{repo}/assignees --jq \".[].login\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process is null) return new List<string>();

            var output = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            using var ctsAssignees = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                await process.WaitForExitAsync(ctsAssignees.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { /* already exited */ }
            }

            if (process.ExitCode != 0)
            {
                Console.Error.WriteLine($"[GithubService] gh api assignees failed for {owner}/{repo}: {stderr}");
                return new List<string>();
            }

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[GithubService] Failed to fetch assignees for {owner}/{repo}: {ex.Message}");
            return new List<string>();
        }
    }
}