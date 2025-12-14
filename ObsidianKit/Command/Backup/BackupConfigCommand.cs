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
        configCommand.AddCommand(CreateIgnoreCommand());

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

    private static Command CreateIgnoreCommand()
    {
        var ignoreCommand = new Command("ignore", "Manage backup ignored paths");
        ignoreCommand.AddCommand(CreateIgnoreAddCommand());
        ignoreCommand.AddCommand(CreateIgnoreRemoveCommand());
        ignoreCommand.AddCommand(CreateIgnoreListCommand());
        return ignoreCommand;
    }

    private static Command CreateIgnoreAddCommand()
    {
        var addCommand = new Command("add", "Add a path to backup ignore list");
        var pathArg = new Argument<string>("path", "Path to ignore during backup");
        addCommand.AddArgument(pathArg);
        addCommand.SetHandler(AddIgnorePath, pathArg);
        return addCommand;

        void AddIgnorePath(string path)
        {
            var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
            config.ignorePaths.Add(path);
            ConfigurationMgr.SaveCommandConfig(config);
            Console.WriteLine($"Added '{path}' to backup ignore list.");
        }
    }

    private static Command CreateIgnoreRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove a path from backup ignore list");
        var pathArg = new Argument<string>("path", "Path to remove from backup ignore list");
        removeCommand.AddArgument(pathArg);
        removeCommand.SetHandler(RemoveIgnorePath, pathArg);
        return removeCommand;

        void RemoveIgnorePath(string path)
        {
            var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
            if (config.ignorePaths.Remove(path))
            {
                ConfigurationMgr.SaveCommandConfig(config);
                Console.WriteLine($"Removed '{path}' from backup ignore list.");
            }
            else
            {
                Console.WriteLine($"Path '{path}' was not found in the backup ignore list.");
            }
        }
    }

    private static Command CreateIgnoreListCommand()
    {
        var listCommand = new Command("list", "List all backup ignored paths");
        listCommand.SetHandler(PrintAllIgnorePaths);
        return listCommand;

        void PrintAllIgnorePaths()
        {
            var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
            Console.WriteLine("Backup Ignored Paths:");
            Console.WriteLine("(These paths are ignored during backup operations)");
            var ignorePaths = config.ignorePaths.ToList();
            if (ignorePaths.Any())
            {
                ignorePaths.Sort();
                for (int i = 0; i < ignorePaths.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {ignorePaths[i]}");
                }
            }
            else
            {
                Console.WriteLine("No paths are currently being ignored during backup.");
            }
        }
    }

    private static void HandleListOption(bool listAll)
    {
        if (!listAll) return;

        var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();

        Console.WriteLine("Backup Configuration:");
        Console.WriteLine("====================");
        Console.WriteLine($"Backup Directory: {config.backupDirectory}");
        Console.WriteLine($"Ignored Paths: {string.Join(", ", config.ignorePaths)}");
    }
}
