using System.CommandLine;
using ObsidianKit.ConsoleUI;
using ObsidianKit.Utilities;

namespace ObsidianKit;

internal static class HexoCommand
{
    internal static Command CreateCommand()
    {
        var hexoCommand = new Command("hexo", "Converts obsidian notes to hexo posts");
        var obsidianOption = new Option<DirectoryInfo>(name: "--obsidian-vault-dir", description: "Path to the Obsidian vault directory",
                                                       getDefaultValue: () => {
                                                           var path = ConfigurationMgr.GetCommandConfig<ConvertConfig>().obsidianVaultPath;
                                                           return new DirectoryInfo(string.IsNullOrWhiteSpace(path) ? "." : path);
                                                       });

        var hexoOption = new Option<DirectoryInfo>(name: "--hexo-posts-dir", description: "Path to the Hexo posts directory", getDefaultValue: () =>
        {
            try
            {
                var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
                var hexoConfig = convertConfig.GetCommandConfig<HexoConfig>();
                var path = hexoConfig.postsPath;
                return new DirectoryInfo(string.IsNullOrWhiteSpace(path) ? "." : path);
            }
            catch
            {
                return new DirectoryInfo(".");
            }
        });

        hexoCommand.AddOption(obsidianOption);
        hexoCommand.AddOption(hexoOption);

        hexoCommand.AddCommand(HexoConfigCommand.CreateCommand());

        hexoCommand.SetHandler(ConvertObsidianKitHexo, obsidianOption, hexoOption);
        return hexoCommand;
    }


    private static void ConvertObsidianKitHexo(DirectoryInfo obsidianVaultDir, DirectoryInfo hexoPostsDir)
    {
        Console.WriteLine($"Obsidian vault path is {obsidianVaultDir.FullName}");
        Console.WriteLine($"Hexo posts path is {hexoPostsDir.FullName}");

        FileSystemUtils.CheckDirectory(obsidianVaultDir, "Obsidian vault directory");
        FileSystemUtils.CheckDirectory(hexoPostsDir, "Hexo posts directory");

        StopWatch.CreateStopWatch("Whole Operation", () =>
        {
            var ObsidianKitHexoHandler = new ObsidianKitHexoHandler(obsidianVaultDir, hexoPostsDir);
            ObsidianKitHexoHandler.Process().Wait();
        });
    }
}
