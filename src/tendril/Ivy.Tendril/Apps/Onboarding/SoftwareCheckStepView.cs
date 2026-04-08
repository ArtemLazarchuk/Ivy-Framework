using System.Diagnostics;
using Ivy.Helpers;

namespace Ivy.Tendril.Apps.Onboarding;

public class SoftwareCheckStepView(IState<int> stepperIndex, IState<Dictionary<string, bool>?> checkResults) : ViewBase
{
    public override object Build()
    {
        var isChecking = UseState(false);

        var hasAnyCodingAgent = checkResults.Value != null
                                && (checkResults.Value["claude"] || checkResults.Value["codex"] ||
                                    checkResults.Value["gemini"]);

        var allRequiredPassed = checkResults.Value != null
                                && checkResults.Value["gh"]
                                && hasAnyCodingAgent
                                && checkResults.Value["git"]
                                && checkResults.Value["powershell"];

        return Layout.Vertical()
               | Text.H2("Required Software")
               | Text.Markdown(
                   """
                   Tendril requires the following software to be installed:

                   **Required:**
                   - **Coding Agent** - At least one of: Claude Code CLI, Codex CLI, or Gemini CLI
                   - **GitHub CLI** - For PR creation and GitHub integration
                   - **Git** - For version control
                   - **PowerShell** - For running promptware and hooks

                   **Optional:**
                   - **Pandoc** - For PDF export functionality
                   """)
               | (checkResults.Value != null
                   ? Layout.Vertical()
                     | Text.H3("Results")
                     | new Table(
                         new TableRow(
                             new TableCell("Software").IsHeader(),
                             new TableCell("Status").IsHeader(),
                             new TableCell("Notes").IsHeader()
                         ).IsHeader(),
                         MakeSoftwareRow(checkResults.Value, "GitHub CLI", "gh", "https://cli.github.com/", true),
                         MakeSoftwareRow(checkResults.Value, "Claude CLI", "claude", "https://docs.anthropic.com/en/docs/claude-code", false),
                         MakeSoftwareRow(checkResults.Value, "Codex CLI", "codex", "https://openai.com/index/codex/", false),
                         MakeSoftwareRow(checkResults.Value, "Gemini CLI", "gemini", "https://github.com/google-gemini/gemini-cli", false),
                         MakeSoftwareRow(checkResults.Value, "Git", "git", "https://git-scm.com/downloads", true),
                         MakeSoftwareRow(checkResults.Value, "PowerShell", "powershell", "https://github.com/PowerShell/PowerShell", true),
                         MakeSoftwareRow(checkResults.Value, "Pandoc (Optional)", "pandoc", "https://pandoc.org/installing.html", false)
                     ).Width(Size.Full())
                   : null!)
               | (checkResults.Value == null
                   ? new Button("Check Software")
                       .Primary()
                       .Large()
                       .Icon(Icons.CheckCheck, Align.Right)
                       .Loading(isChecking.Value)
                       .Disabled(isChecking.Value)
                       .OnClick(async () => await CheckSoftware())
                   : allRequiredPassed
                       ? new Button("Continue")
                           .Primary()
                           .Large()
                           .Icon(Icons.ArrowRight, Align.Right)
                           .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1))
                       : Layout.Vertical()
                         | Text.Warning(
                             "Please install all required software before continuing. At least one coding agent (Claude, Codex, or Gemini), GitHub CLI, Git, and PowerShell must be installed.")
                            | new Button("Check Again")
                                .Outline()
                                .Icon(Icons.CheckCheck, Align.Right)
                                .OnClick(async () => await CheckSoftware())
               );

        async Task CheckSoftware()
        {
            isChecking.Set(true);
            var results = new Dictionary<string, bool>
            {
                ["gh"] = await CheckCommand("gh", "--version"),
                ["claude"] = await CheckCommand("claude", "--version"),
                ["codex"] = await CheckCommand("codex", "--version"),
                ["gemini"] = await CheckCommand("gemini", "--version"),
                ["git"] = await CheckCommand("git", "--version"),
                ["powershell"] = await CheckCommand("pwsh", "-Version")
                                 || await CheckCommand("powershell", "-Version"),
                ["pandoc"] = await CheckCommand("pandoc", "--version")
            };

            checkResults.Set(results);
            isChecking.Set(false);
        }
    }

    private static TableRow MakeSoftwareRow(
        Dictionary<string, bool> results,
        string displayName,
        string key,
        string installUrl,
        bool isRequired)
    {
        var installed = results[key];
        var statusText = installed
            ? "✅ Installed"
            : isRequired ? "❌ Not Found" : "❌ Not Installed";

        return new TableRow(
            new TableCell(displayName),
            new TableCell(statusText),
            installed
                ? new TableCell("")
                : new TableCell(new Button("Install").Inline().Url(installUrl))
        );
    }

    private static async Task<bool> CheckCommand(string fileName, string arguments)
    {
        try
        {
            return await Task.Run(() =>
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : fileName,
                    Arguments = OperatingSystem.IsWindows() ? $"/c \"{fileName}\" {arguments}" : arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.WaitForExitOrKill(10000);
                return proc?.ExitCode == 0;
            });
        }
        catch
        {
            return false;
        }
    }
}
