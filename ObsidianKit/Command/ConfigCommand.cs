using System.CommandLine;
using System.Linq;

namespace ObsidianKit;

public static class ConfigCommand
{
    internal static Command CreateCommand()
    {
        var configCommand = new Command("config", "Manage configuration settings");

        var configPath = ConfigurationMgr.configurationPath;
        configCommand.Description = $"Manage configuration settings\n\nConfiguration file location:\n{configPath}";

        var listOption = new Option<bool>("--list", "List configuration settings");
        var allOption = new Option<bool>("--a", "Show all command configurations (use with --list)");

        configCommand.AddOption(listOption);
        configCommand.AddOption(allOption);

        configCommand.SetHandler(HandleListOption, listOption, allOption);

        configCommand.AddCommand(ConfigPathCommand.CreateObsidianVaultDirCommand());
        configCommand.AddCommand(ConfigIgnoreCommand.CreateCommand());
        configCommand.AddCommand(ConfigImportExportCommand.CreateExportCommand());
        configCommand.AddCommand(ConfigImportExportCommand.CreateImportCommand());

        return configCommand;
    }

    private static void HandleListOption(bool listAll, bool showAllCommands)
    {
        if (!listAll) return;

        var config = ConfigurationMgr.configuration;
        var convertConfig = config.GetConvertConfig();

        Console.WriteLine("Global Configuration:");
        Console.WriteLine("=====================");
        Console.WriteLine($"Obsidian Vault Path: {convertConfig.obsidianVaultPath ?? "Not set"}");
        Console.WriteLine($"Ignored Paths: {(convertConfig.ignoresPaths.Any() ? string.Join(", ", convertConfig.ignoresPaths) : "None")}");

        if (showAllCommands)
        {
            Console.WriteLine();
            Console.WriteLine("Command Configurations:");
            Console.WriteLine("=======================");

            foreach (var commandConfig in config.commandConfigs.Values)
            {
                Console.WriteLine($"\n--- {commandConfig.CommandName} ---");
                commandConfig.DisplayConfiguration();
            }
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Note: Use 'obsidiankit config --list --a' to see all command configurations");
        }
    }
}
