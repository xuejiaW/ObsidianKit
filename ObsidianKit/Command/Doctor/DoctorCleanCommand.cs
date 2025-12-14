using System.CommandLine;
using ObsidianKit.Utilities.Obsidian;

namespace ObsidianKit;

internal static class DoctorCleanCommand
{
    internal static Command CreateCommand()
    {
        var cleanCommand = new Command("clean", "Find and remove unreferenced image files");

        var vaultDirOption = new Option<DirectoryInfo>(
            "--vault-dir",
            "Path to Obsidian vault (uses configured path if not specified)");

        cleanCommand.AddOption(vaultDirOption);
        cleanCommand.SetHandler(HandleClean, vaultDirOption);

        return cleanCommand;
    }

    private static void HandleClean(DirectoryInfo vaultDir)
    {
        var vaultPath = GetVaultPath(vaultDir);

        Console.WriteLine($"Scanning vault: {vaultPath}");
        Console.WriteLine("This may take a while for large vaults...");
        Console.WriteLine();

        var config = ConfigurationMgr.configuration;
        var unreferencedImages = ObsidianCleanUtils.FindUnreferencedImages(vaultPath, config.globalIgnoresPaths);

        if (!unreferencedImages.Any())
        {
            Console.WriteLine("No unreferenced images found. Your vault is clean!");
            return;
        }

        Console.WriteLine($"Found {unreferencedImages.Count} unreferenced image(s):");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        var totalSize = 0L;
        for (int i = 0; i < unreferencedImages.Count; i++)
        {
            var img = unreferencedImages[i];
            var relativePath = Path.GetRelativePath(vaultPath, img.filePath);
            var size = FormatFileSize(img.fileSize);
            Console.WriteLine($"[{i + 1}] {relativePath,-60} {size,10}");
            totalSize += img.fileSize;
        }

        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"Total size: {FormatFileSize(totalSize)}");
        Console.WriteLine();

        Console.Write("Move these files to .trash folder? (y/N): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (response != "y" && response != "yes")
        {
            Console.WriteLine("Operation cancelled.");
            return;
        }

        var trashDir = Path.Combine(vaultPath, ".trash");
        if (!Directory.Exists(trashDir))
            Directory.CreateDirectory(trashDir);

        var movedCount = 0;
        var movedSize = 0L;

        foreach (var img in unreferencedImages)
        {
            try
            {
                var fileName = Path.GetFileName(img.filePath);
                var targetPath = Path.Combine(trashDir, fileName);
                
                if (File.Exists(targetPath))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    targetPath = Path.Combine(trashDir, $"{nameWithoutExt}_{timestamp}{ext}");
                }

                File.Move(img.filePath, targetPath);
                movedCount++;
                movedSize += img.fileSize;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving {Path.GetFileName(img.filePath)}: {ex.Message}");
            }
        }

        var trashSize = Directory.GetFiles(trashDir, "*.*", SearchOption.AllDirectories)
            .Sum(f => new FileInfo(f).Length);

        Console.WriteLine();
        Console.WriteLine($"Moved {movedCount} file(s) to .trash, freed {FormatFileSize(movedSize)}");
        Console.WriteLine($"Current .trash size: {FormatFileSize(trashSize)}");
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
