using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public static class BackupConfigCommand
{
    internal static Command CreateCommand()
    {
        var configCommand = new Command("config", "Manage backup configuration");

        var listOption = new Option<bool>("--list", "List backup configuration settings");
        configCommand.AddOption(listOption);

        configCommand.AddCommand(CreateBackupDirCommand());

        configCommand.SetHandler(HandleListOption, listOption);

        return configCommand;
    }

    private static Command CreateBackupDirCommand()
    {
        var backupDirCmd = new Command("backup-dir", "Set backup directory");
        var dirArg = new Argument<DirectoryInfo>("directory", "Path to backup directory");
        backupDirCmd.AddArgument(dirArg);
        backupDirCmd.SetHandler(SetBackupDir, dirArg);
        return backupDirCmd;

        void SetBackupDir(DirectoryInfo directory)
        {
            FileSystemUtils.EnsureDirectoryExists(directory.FullName);

            var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
            config.backupDirectory = directory.FullName;
            ConfigurationMgr.SaveCommandConfig(config);
            Console.WriteLine("Backup directory has been set.");
        }
    }

    private static void HandleListOption(bool listAll)
    {
        if (!listAll) return;

        var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();

        Console.WriteLine("Backup Configuration:");
        Console.WriteLine("====================");
        Console.WriteLine($"Backup Directory: {config.backupDirectory}");
    }
}
