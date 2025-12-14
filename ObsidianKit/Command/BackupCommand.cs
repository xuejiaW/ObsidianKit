using System.CommandLine;
using System.IO.Compression;
using ObsidianKit.ConsoleUI;

namespace ObsidianKit;

internal static class BackupCommand
{
    internal static Command CreateCommand()
    {
        var backupCommand = new Command("backup", "Backup and restore Obsidian vault");

        var vaultDirOption = new Option<DirectoryInfo>(
            "--vault-dir",
            "Path to Obsidian vault (uses configured path if not specified)");

        backupCommand.AddOption(vaultDirOption);
        backupCommand.SetHandler(HandleBackup, vaultDirOption);

        backupCommand.AddCommand(CreateListCommand());
        backupCommand.AddCommand(CreateRestoreCommand());
        backupCommand.AddCommand(CreateRemoveCommand());
        backupCommand.AddCommand(BackupConfigCommand.CreateCommand());

        return backupCommand;
    }

    private static Command CreateListCommand()
    {
        var listCommand = new Command("list", "List all backups");
        listCommand.SetHandler(HandleList);
        return listCommand;
    }

    private static Command CreateRestoreCommand()
    {
        var restoreCommand = new Command("restore", "Restore a backup");

        var nameArg = new Argument<string>("backup-name", "Name of the backup to restore (without .zip extension)");
        var targetDirOption = new Option<DirectoryInfo>("--target-dir", "Target directory for restoration");

        restoreCommand.AddArgument(nameArg);
        restoreCommand.AddOption(targetDirOption);
        restoreCommand.SetHandler(HandleRestore, nameArg, targetDirOption);

        return restoreCommand;
    }

    private static Command CreateRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove backups");

        var nameArg = new Argument<string>("backup-name", "Name or indices (e.g., '1,3' or '1~3' or backup name)");
        removeCommand.AddArgument(nameArg);
        removeCommand.SetHandler(HandleRemove, nameArg);

        return removeCommand;
    }

    private static void HandleBackup(DirectoryInfo vaultDir)
    {
        var vaultPath = GetVaultPath(vaultDir);
        var backupConfig = ConfigurationMgr.GetCommandConfig<BackupConfig>();
        var globalConfig = ConfigurationMgr.configuration;

        if (!Directory.Exists(backupConfig.backupDirectory))
            Directory.CreateDirectory(backupConfig.backupDirectory);

        var vaultName = Path.GetFileName(vaultPath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{vaultName}_{timestamp}.zip";
        var backupFilePath = Path.Combine(backupConfig.backupDirectory, backupFileName);

        Console.WriteLine($"Creating backup of: {vaultPath}");
        Console.WriteLine($"Backup file: {backupFilePath}");
        Console.WriteLine();

        try
        {
            var allFiles = Directory.GetFiles(vaultPath, "*.*", SearchOption.AllDirectories);
            var ignorePaths = globalConfig.globalIgnoresPaths;

            var filesToBackup = allFiles
                .Where(file => !ignorePaths.Any(ignore =>
                    Path.GetRelativePath(vaultPath, file).Contains(ignore, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Progress.CreateProgress($"Backing up {filesToBackup.Count} files...", async progressTask =>
            {
                progressTask.MaxValue = filesToBackup.Count;

                await Task.Run(() =>
                {
                    using (var archive = ZipFile.Open(backupFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var file in filesToBackup)
                        {
                            var relativePath = Path.GetRelativePath(vaultPath, file);
                            archive.CreateEntryFromFile(file, relativePath, CompressionLevel.Fastest);

                            progressTask.Increment(1);
                        }
                    }
                });
            }).Wait();

            var fileInfo = new FileInfo(backupFilePath);
            Console.WriteLine($"\nBackup created successfully. Size: {FormatFileSize(fileInfo.Length)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating backup: {ex.Message}");
        }
    }

    private static void HandleList()
    {
        var backupFiles = GetBackupFiles();

        if (backupFiles == null || !backupFiles.Any())
            return;

        var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
        Console.WriteLine($"Backups in: {config.backupDirectory}");
        Console.WriteLine("==================================================");

        for (int i = 0; i < backupFiles.Count; i++)
        {
            var file = backupFiles[i];
            var name = Path.GetFileNameWithoutExtension(file.Name);
            var size = FormatFileSize(file.Length);
            var date = file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{i + 1}] {name,-45} {size,10}  {date}");
        }

        Console.WriteLine($"\nTotal: {backupFiles.Count} backup(s)");
    }

    private static void HandleRestore(string backupName, DirectoryInfo targetDir)
    {
        var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
        var backupFilePath = Path.Combine(config.backupDirectory, $"{backupName}.zip");

        if (!File.Exists(backupFilePath))
        {
            Console.WriteLine($"Backup not found: {backupName}");
            Console.WriteLine("Use 'obk backup list' to see available backups.");
            return;
        }

        string targetPath;
        if (targetDir != null)
        {
            targetPath = targetDir.FullName;
        }
        else
        {
            var vaultName = backupName.Substring(0, backupName.LastIndexOf('_'));
            targetPath = Path.Combine(Directory.GetCurrentDirectory(), $"{vaultName}_restored");
        }

        if (Directory.Exists(targetPath) && Directory.EnumerateFileSystemEntries(targetPath).Any())
        {
            Console.WriteLine($"Warning: Target directory is not empty: {targetPath}");
            Console.Write("Continue? (y/N): ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Restoration cancelled.");
                return;
            }
        }

        Console.WriteLine($"Restoring backup: {backupName}");
        Console.WriteLine($"Target directory: {targetPath}");

        try
        {
            ZipFile.ExtractToDirectory(backupFilePath, targetPath, true);
            Console.WriteLine("Backup restored successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring backup: {ex.Message}");
        }
    }

    private static void HandleRemove(string input)
    {
        var backupFiles = GetBackupFiles();
        if (backupFiles == null || !backupFiles.Any())
            return;

        var filesToDelete = new List<FileInfo>();

        if (input.Contains(',') || input.Contains('~'))
        {
            var indices = ParseIndices(input, backupFiles.Count);
            if (indices == null || !indices.Any())
            {
                Console.WriteLine("Invalid index format. Use formats like: 1,3 or 1~3 or 1,2,5~7");
                return;
            }

            filesToDelete.AddRange(indices.Select(i => backupFiles[i - 1]));
        }
        else if (int.TryParse(input, out int index))
        {
            if (index < 1 || index > backupFiles.Count)
            {
                Console.WriteLine($"Invalid index. Please use 1-{backupFiles.Count}");
                return;
            }
            filesToDelete.Add(backupFiles[index - 1]);
        }
        else
        {
            var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();
            var backupFilePath = Path.Combine(config.backupDirectory, $"{input}.zip");
            if (!File.Exists(backupFilePath))
            {
                Console.WriteLine($"Backup not found: {input}");
                return;
            }
            filesToDelete.Add(new FileInfo(backupFilePath));
        }

        Console.WriteLine($"Backups to delete ({filesToDelete.Count}):");
        foreach (var file in filesToDelete)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);
            var size = FormatFileSize(file.Length);
            var date = file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"  • {name,-45} {size,10}  {date}");
        }

        Console.Write($"\nAre you sure you want to delete {filesToDelete.Count} backup(s)? (y/N): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (response != "y" && response != "yes")
        {
            Console.WriteLine("Deletion cancelled.");
            return;
        }

        int deletedCount = 0;
        foreach (var file in filesToDelete)
        {
            try
            {
                File.Delete(file.FullName);
                deletedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {file.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"{deletedCount} backup(s) deleted successfully.");
    }

    private static List<int> ParseIndices(string input, int maxIndex)
    {
        var indices = new HashSet<int>();

        try
        {
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains('~'))
                {
                    var range = trimmed.Split('~', StringSplitOptions.RemoveEmptyEntries);
                    if (range.Length != 2)
                        return null;

                    if (!int.TryParse(range[0].Trim(), out int start) || !int.TryParse(range[1].Trim(), out int end))
                        return null;

                    if (start < 1 || end > maxIndex || start > end)
                        return null;

                    for (int i = start; i <= end; i++)
                        indices.Add(i);
                }
                else
                {
                    if (!int.TryParse(trimmed, out int index))
                        return null;

                    if (index < 1 || index > maxIndex)
                        return null;

                    indices.Add(index);
                }
            }
        }
        catch
        {
            return null;
        }

        return indices.OrderBy(i => i).ToList();
    }

    private static List<FileInfo> GetBackupFiles()
    {
        var config = ConfigurationMgr.GetCommandConfig<BackupConfig>();

        if (!Directory.Exists(config.backupDirectory))
        {
            Console.WriteLine("No backups found. Backup directory does not exist.");
            return null;
        }

        var backupFiles = Directory.GetFiles(config.backupDirectory, "*.zip")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        if (!backupFiles.Any())
        {
            Console.WriteLine("No backups found.");
            return null;
        }

        return backupFiles;
    }

    private static string GetVaultPath(DirectoryInfo vaultDir)
    {
        if (vaultDir != null && vaultDir.Exists)
            return vaultDir.FullName;

        var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
        if (!string.IsNullOrEmpty(convertConfig.obsidianVaultPath))
            return convertConfig.obsidianVaultPath;

        throw new ArgumentException("Vault directory not specified and not configured. Use --vault-dir or configure with 'obk config obsidian-vault-dir'");
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
