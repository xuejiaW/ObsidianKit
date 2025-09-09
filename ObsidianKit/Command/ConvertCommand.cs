using System.CommandLine;

namespace ObsidianKit;

internal static class ConvertCommand
{
    internal static Command CreateCommand()
    {
        var convertCommand = new Command("convert", "Convert Obsidian notes to various formats");
        
        // Add subcommands
        convertCommand.AddCommand(HexoCommand.CreateCommand());
        convertCommand.AddCommand(CompatCommand.CreateCommand());
        convertCommand.AddCommand(ZhihuCommand.CreateCommand());
        
        return convertCommand;
    }
}
