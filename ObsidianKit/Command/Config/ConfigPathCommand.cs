using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public static class ConfigPathCommand
{
    internal static Command CreateObsidianVaultDirCommand()
    {
        var setObsidianVaultDirCmd = new Command("obsidian-vault-dir", "Set Obsidian vault directory");
        var obsidianVaultDirArg = new Argument<DirectoryInfo>("directory", "Path to Obsidian vault directory");
        setObsidianVaultDirCmd.AddArgument(obsidianVaultDirArg);
        setObsidianVaultDirCmd.SetHandler(SetObsidianVaultDir, obsidianVaultDirArg);
        return setObsidianVaultDirCmd;

        void SetObsidianVaultDir(DirectoryInfo obsidianVaultDir)
        {
            FileSystemUtils.CheckDirectory(obsidianVaultDir, "Obsidian vault directory");
            var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
            convertConfig.obsidianVaultPath = obsidianVaultDir.FullName;
            ConfigurationMgr.SaveCommandConfig(convertConfig);
            Console.WriteLine("Obsidian vault directory has been set.");
        }
    }
}
