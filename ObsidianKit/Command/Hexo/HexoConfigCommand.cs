using System.CommandLine;

namespace ObsidianKit;

public static class HexoConfigCommand
{
    internal static Command CreateCommand()
    {
        var configCommand = new Command("config", "Manage Hexo command configuration");
        
        var listOption = new Option<bool>("--list", "List Hexo configuration settings");
        configCommand.AddOption(listOption);
        
        configCommand.AddCommand(HexoConfigPostsDirCommand.CreateCommand());
        
        // Set handler only when no subcommands are invoked
        configCommand.SetHandler(HandleListOption, listOption);
        
        return configCommand;
    }

    private static void HandleListOption(bool listAll)
    {
        if (!listAll) return;
        
        var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
        var config = convertConfig.GetCommandConfig<HexoConfig>();
        
        Console.WriteLine("Hexo Configuration:");
        Console.WriteLine("==================");
        Console.WriteLine($"Posts Directory: {config.postsPath ?? "Not set"}");
    }
}
