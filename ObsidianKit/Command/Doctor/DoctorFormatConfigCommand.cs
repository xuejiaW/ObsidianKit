using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public static class DoctorFormatConfigCommand
{
    internal static Command CreateCommand()
    {
        var configCommand = new Command("config", "Manage format check configuration");

        var listOption = new Option<bool>("--list", "List format configuration settings");
        configCommand.AddOption(listOption);

        configCommand.AddCommand(CreateIgnoreCommand());

        configCommand.SetHandler(HandleListOption, listOption);

        return configCommand;
    }

    private static void HandleListOption(bool list)
    {
        if (list)
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorFormatConfig>();
            config.DisplayConfiguration();
        }
    }

    // ============ ignore subcommand ============
    private static Command CreateIgnoreCommand()
    {
        var ignoreCommand = new Command("ignore", "Manage file/folder ignore patterns for format checking");
        ignoreCommand.AddCommand(CreateIgnoreAddCommand());
        ignoreCommand.AddCommand(CreateIgnoreRemoveCommand());
        ignoreCommand.AddCommand(CreateIgnoreListCommand());
        return ignoreCommand;
    }

    private static Command CreateIgnoreAddCommand()
    {
        var addCommand = new Command("add", "Add a pattern to ignore list");
        var patternArg = new Argument<string>("pattern", "Pattern to ignore (supports wildcards like '**/*.tmp')");
        addCommand.AddArgument(patternArg);
        addCommand.SetHandler(AddIgnorePattern, patternArg);
        return addCommand;

        void AddIgnorePattern(string pattern)
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorFormatConfig>();
            if (config.ignorePatterns.Add(pattern))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Added pattern '{pattern}' to format ignore list.");
                
                // Sync to user's config file
                SyncToVaultConfig();
            }
            else
            {
                Console.WriteLine($"Pattern '{pattern}' already exists in format ignore list.");
            }
        }
    }

    private static Command CreateIgnoreRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove a pattern from ignore list");
        var patternArg = new Argument<string>("pattern", "Pattern to remove from ignore list");
        removeCommand.AddArgument(patternArg);
        removeCommand.SetHandler(RemoveIgnorePattern, patternArg);
        return removeCommand;

        void RemoveIgnorePattern(string pattern)
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorFormatConfig>();
            if (config.ignorePatterns.Remove(pattern))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Removed pattern '{pattern}' from format ignore list.");
                
                // Sync to user's config file
                SyncToVaultConfig();
            }
            else
            {
                Console.WriteLine($"Pattern '{pattern}' was not found in format ignore list.");
            }
        }
    }

    private static Command CreateIgnoreListCommand()
    {
        var listCommand = new Command("list", "List all ignore patterns");
        listCommand.SetHandler(PrintAllIgnorePatterns);
        return listCommand;

        void PrintAllIgnorePatterns()
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorFormatConfig>();
            Console.WriteLine($"Format Check Ignore Patterns ({config.ignorePatterns.Count}):");
            if (config.ignorePatterns.Any())
            {
                foreach (var pattern in config.ignorePatterns.OrderBy(p => p))
                {
                    Console.WriteLine($"  - {pattern}");
                }
            }
            else
            {
                Console.WriteLine("  (none)");
            }
        }
    }
    
    /// <summary>
    /// Sync ignore patterns to vault's config file using shared utility
    /// </summary>
    private static void SyncToVaultConfig()
    {
        try
        {
            // Get vault path from config
            var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
            if (string.IsNullOrEmpty(convertConfig.obsidianVaultPath))
                return;
            
            // Use shared utility for sync
            MarkdownlintConfigSync.SyncIgnorePatternsToConfigFile(
                convertConfig.obsidianVaultPath, 
                showMessage: true);
        }
        catch
        {
            // Ignore sync errors
        }
    }
}
