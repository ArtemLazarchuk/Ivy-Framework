using Ivy.Helpers;
using Ivy.Tendril.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Tendril.Apps.Onboarding;

public class CompleteStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var isProcessing = UseState(false);
        var error = UseState<string?>(null);
        var config = UseService<IConfigService>();
        var services = UseService<IServiceProvider>();
        var navigator = UseNavigation();

        async Task OnComplete()
        {
            isProcessing.Set(true);
            error.Set(null);

            try
            {
                var tendrilHome = config.GetPendingTendrilHome();

                if (string.IsNullOrEmpty(tendrilHome))
                {
                    error.Set("Tendril home path not set");
                    isProcessing.Set(false);
                    return;
                }

                // Create directory structure
                Directory.CreateDirectory(tendrilHome);
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Inbox"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Plans"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Trash"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Promptwares"));
                Directory.CreateDirectory(Path.Combine(tendrilHome, "Hooks"));

                // Copy template or create basic config
                var projectDir = Path.GetDirectoryName(System.AppContext.BaseDirectory); // Go up from bin/Debug/...
                while (projectDir != null && !File.Exists(Path.Combine(projectDir, "example.config.yaml")))
                    projectDir = Path.GetDirectoryName(projectDir);

                var exampleConfigPath = projectDir != null
                    ? Path.Combine(projectDir, "example.config.yaml")
                    : Path.Combine(System.AppContext.BaseDirectory, "example.config.yaml");

                var configPath = Path.Combine(tendrilHome, "config.yaml");

                if (File.Exists(exampleConfigPath))
                {
                    var exampleContent = await FileHelper.ReadAllTextAsync(exampleConfigPath);
                    await FileHelper.WriteAllTextAsync(configPath, exampleContent);
                }
                else if (!File.Exists(configPath))
                {
                    // Create a basic config.yaml only if it doesn't exist
                    var basicConfig = "codingAgent: claude\n" +
                                      "jobTimeout: 30\n" +
                                      "staleOutputTimeout: 10\n" +
                                      "projects: []\n" +
                                      "verifications: []\n";
                    await FileHelper.WriteAllTextAsync(configPath, basicConfig);
                }

                // Set environment variable for current session
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);

                // Persist to shell for Mac users
                if (OperatingSystem.IsMacOS())
                    try
                    {
                        var zshrc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            ".zshrc");
                        var exportLine = $"export TENDRIL_HOME=\"{tendrilHome}\"";
                        if (File.Exists(zshrc))
                        {
                            var content = await FileHelper.ReadAllTextAsync(zshrc);
                            if (!content.Contains(exportLine))
                                await File.AppendAllLinesAsync(zshrc, new[] { "", "# Tendril Home", exportLine });
                        }
                    }
                    catch
                    {
                        /* Best effort */
                    }

                // Mark onboarding complete (this reloads config from the file we just wrote)
                config.CompleteOnboarding(tendrilHome);

                // Add pending verification definitions to global config
                var pendingDefinitions = config.GetPendingVerificationDefinitions();
                if (pendingDefinitions != null)
                    foreach (var def in pendingDefinitions)
                        if (!config.Settings.Verifications.Any(v => v.Name == def.Name))
                            config.Settings.Verifications.Add(def);

                // Add pending project if one was configured
                var pendingProject = config.GetPendingProject();
                if (pendingProject != null)
                {
                    config.Settings.Projects.Add(pendingProject);
                    config.SaveSettings();
                }

                // Initialize database and start background services now that TendrilHome is set
                services.GetRequiredService<IPlanWatcherService>();
                services.GetRequiredService<IInboxWatcherService>();
                services.GetRequiredService<WorktreeCleanupService>().Start();

                var syncService = services.GetRequiredService<PlanDatabaseSyncService>();
                _ = Task.Run(syncService.PerformInitialSync);

                // Navigate to SetupApp
                navigator.Navigate<SetupApp>();
            }
            catch (Exception ex)
            {
                error.Set($"Failed to complete setup: {ex.Message}");
                isProcessing.Set(false);
            }
        }

        return Layout.Vertical()
               | Text.H2("Ready to Go!")
               | Text.Markdown(
                   "We'll now:\n" +
                   "- Create the necessary folder structure\n" +
                   "- Set up your configuration file\n" +
                   "- Initialize Tendril with default settings\n\n" +
                   "Click 'Complete Setup' to finish.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | new Button("Complete Setup")
                   .Primary()
                   .Large()
                   .Icon(Icons.Check, Align.Right)
                   .Disabled(isProcessing.Value)
                   .Loading(isProcessing.Value)
                   .OnClick(async () => await OnComplete());
    }
}
