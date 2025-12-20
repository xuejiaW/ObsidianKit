using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public static class DoctorBloatConfigCommand
{
    internal static Command CreateCommand()
    {
        var configCommand = new Command("config", "Manage bloat check configuration");

        var listOption = new Option<bool>("--list", "List bloat configuration settings");
        configCommand.AddOption(listOption);

        configCommand.AddCommand(CreateIgnoreCommand());
        configCommand.AddCommand(CreateLimitCommand());
        configCommand.AddCommand(CreateDefaultSizeCommand());

        configCommand.SetHandler(HandleListOption, listOption);

        return configCommand;
    }

    private static void HandleListOption(bool list)
    {
        if (list)
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            config.DisplayConfiguration();
        }
    }

    // ============ ignore subcommand ============
    private static Command CreateIgnoreCommand()
    {
        var ignoreCommand = new Command("ignore", "Manage file/folder ignore patterns");
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
            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            if (config.ignorePatterns.Add(pattern))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Added pattern '{pattern}' to ignore list.");
            }
            else
            {
                Console.WriteLine($"Pattern '{pattern}' already exists in ignore list.");
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
            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            if (config.ignorePatterns.Remove(pattern))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Removed pattern '{pattern}' from ignore list.");
            }
            else
            {
                Console.WriteLine($"Pattern '{pattern}' was not found in ignore list.");
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
            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            Console.WriteLine($"Ignore Patterns ({config.ignorePatterns.Count}):");
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

    // ============ limit subcommand ============
    private static Command CreateLimitCommand()
    {
        var limitCommand = new Command("limit", "Manage file size limits by extension");
        limitCommand.AddCommand(CreateLimitSetCommand());
        limitCommand.AddCommand(CreateLimitRemoveCommand());
        limitCommand.AddCommand(CreateLimitListCommand());
        return limitCommand;
    }

    private static Command CreateLimitSetCommand()
    {
        var setCommand = new Command("set", "Set size limit for a file extension");
        var extArg = new Argument<string>("extension", "File extension (e.g., '.png', '.mp4')");
        var sizeArg = new Argument<string>("size", "Maximum size (e.g., '1MB', '500KB', '10MB')");
        setCommand.AddArgument(extArg);
        setCommand.AddArgument(sizeArg);
        setCommand.SetHandler(SetSizeLimit, extArg, sizeArg);
        return setCommand;

        void SetSizeLimit(string extension, string sizeStr)
        {
            try
            {
                if (!extension.StartsWith("."))
                    extension = "." + extension;

                var size = FileSizeUtils.ParseSize(sizeStr);
                var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
                
                config.fileSizeLimits[extension.ToLower()] = size;
                ConfigurationMgr.SaveCommandConfig(config);
                
                Console.WriteLine($"Set size limit for '{extension}' to {FileSizeUtils.FormatSize(size)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static Command CreateLimitRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove size limit for a file extension");
        var extArg = new Argument<string>("extension", "File extension to remove limit for");
        removeCommand.AddArgument(extArg);
        removeCommand.SetHandler(RemoveSizeLimit, extArg);
        return removeCommand;

        void RemoveSizeLimit(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            if (config.fileSizeLimits.Remove(extension.ToLower()))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Removed size limit for '{extension}'");
            }
            else
            {
                Console.WriteLine($"No size limit found for '{extension}'");
            }
        }
    }

    private static Command CreateLimitListCommand()
    {
        var listCommand = new Command("list", "List all file size limits");
        listCommand.SetHandler(PrintAllLimits);
        return listCommand;

        void PrintAllLimits()
        {
            var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
            Console.WriteLine($"File Size Limits ({config.fileSizeLimits.Count}):");
            foreach (var (ext, size) in config.fileSizeLimits.OrderBy(x => x.Key))
            {
                Console.WriteLine($"  {ext,-10} : {FileSizeUtils.FormatSize(size)}");
            }
        }
    }

    // ============ default-size subcommand ============
    private static Command CreateDefaultSizeCommand()
    {
        var defaultSizeCommand = new Command("default-size", "Set default maximum file size");
        var sizeArg = new Argument<string>("size", "Default maximum size (e.g., '2MB', '1GB')");
        defaultSizeCommand.AddArgument(sizeArg);
        defaultSizeCommand.SetHandler(SetDefaultSize, sizeArg);
        return defaultSizeCommand;

        void SetDefaultSize(string sizeStr)
        {
            try
            {
                var size = FileSizeUtils.ParseSize(sizeStr);
                var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
                
                config.defaultMaxSize = size;
                ConfigurationMgr.SaveCommandConfig(config);
                
                Console.WriteLine($"Set default maximum file size to {FileSizeUtils.FormatSize(size)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
