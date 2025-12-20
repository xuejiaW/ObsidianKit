using System.CommandLine;
using ObsidianKit.ConsoleUI;
using ObsidianKit.Utilities;

namespace ObsidianKit;

internal static class DoctorBloatCommand
{
    internal static Command CreateCommand()
    {
        var bloatCommand = new Command("bloat", "Check for oversized files in Obsidian vault");

        var vaultDirOption = new Option<DirectoryInfo>(
            "--vault-dir",
            "Path to Obsidian vault (uses configured path if not specified)");

        bloatCommand.AddOption(vaultDirOption);
        bloatCommand.SetHandler(HandleBloatCheck, vaultDirOption);

        // Add config subcommand
        bloatCommand.AddCommand(DoctorBloatConfigCommand.CreateCommand());

        return bloatCommand;
    }

    private static void HandleBloatCheck(DirectoryInfo vaultDir)
    {
        var vaultPath = GetVaultPath(vaultDir);
        var config = ConfigurationMgr.GetCommandConfig<DoctorBloatConfig>();
        var globalConfig = ConfigurationMgr.configuration;

        Console.WriteLine($"Scanning Obsidian vault: {vaultPath}");
        Console.WriteLine("Checking for oversized files...");
        Console.WriteLine("====================================");
        Console.WriteLine();

        var allFiles = Directory.GetFiles(vaultPath, "*.*", SearchOption.AllDirectories);
        var oversizedFiles = new List<(string path, long size, long limit)>();

        foreach (var file in allFiles)
        {
            var relativePath = Path.GetRelativePath(vaultPath, file);

            // Check if should be ignored
            if (ShouldIgnore(relativePath, globalConfig.globalIgnoresPaths, config.ignorePatterns))
                continue;

            var fileInfo = new FileInfo(file);
            var extension = fileInfo.Extension.ToLower();

            // Get size limit for this file type
            var sizeLimit = config.fileSizeLimits.TryGetValue(extension, out var limit)
                ? limit
                : config.defaultMaxSize;

            if (fileInfo.Length > sizeLimit)
            {
                oversizedFiles.Add((relativePath, fileInfo.Length, sizeLimit));
            }
        }

        if (oversizedFiles.Any())
        {
            Console.WriteLine($"Found {oversizedFiles.Count} oversized file(s):");
            Console.WriteLine();

            // Calculate column widths
            var maxPathLength = oversizedFiles.Max(f => f.path.Length);
            var pathWidth = Math.Max(Math.Min(maxPathLength, 60), 20); // Min 20, max 60
            var sizeWidth = 12;
            var limitWidth = 12;
            var percentWidth = 10;

            // Prepare header
            var header = string.Format("{0}  {1}  {2}  {3}",
                "File".PadRight(pathWidth),
                "Size".PadLeft(sizeWidth),
                "Limit".PadLeft(limitWidth),
                "Exceeded".PadLeft(percentWidth));
            var separator = new string('-', header.Length);

            // Prepare all rows
            var rows = new List<string> { header, separator };
            
            foreach (var (path, size, limit) in oversizedFiles.OrderByDescending(f => f.size))
            {
                var displayPath = path.Length > pathWidth ? "..." + path.Substring(path.Length - pathWidth + 3) : path;
                var sizeStr = FileSizeUtils.FormatSize(size);
                var limitStr = FileSizeUtils.FormatSize(limit);
                var percentExceeded = ((double)(size - limit) / limit * 100).ToString("0.0") + "%";

                var row = string.Format("{0}  {1}  {2}  {3}",
                    displayPath.PadRight(pathWidth),
                    sizeStr.PadLeft(sizeWidth),
                    limitStr.PadLeft(limitWidth),
                    percentExceeded.PadLeft(percentWidth));
                rows.Add(row);
            }

            // Display with pagination
            Pager.DisplayWithPagination(rows);

            Console.WriteLine();
            Console.WriteLine($"Total: {oversizedFiles.Count} file(s), Total excess: {FileSizeUtils.FormatSize(oversizedFiles.Sum(f => f.size - f.limit))}");
        }
        else
        {
            Console.WriteLine("✓ No oversized files found!");
        }
    }

    private static bool ShouldIgnore(string relativePath,
        HashSet<string> globalIgnores,
        HashSet<string> configIgnores)
    {
        var normalizedPath = relativePath.Replace('\\', '/');

        // Check global ignores
        foreach (var ignore in globalIgnores)
        {
            if (normalizedPath.StartsWith(ignore.Replace('\\', '/').TrimEnd('/') + "/"))
                return true;
        }

        // Check configured ignore patterns
        foreach (var pattern in configIgnores)
        {
            if (IsMatch(normalizedPath, pattern))
                return true;
        }

        return false;
    }

    private static bool IsMatch(string path, string pattern)
    {
        var normalizedPattern = pattern.Replace('\\', '/');
        
        // Convert glob pattern to regex
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(normalizedPattern)
            .Replace("\\*\\*", ".*")  // ** matches any characters including /
            .Replace("\\*", "[^/]*")   // * matches any characters except /
            .Replace("\\?", "[^/]")    // ? matches single character except /
            + "$";
        
        try
        {
            return System.Text.RegularExpressions.Regex.IsMatch(path, regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        catch
        {
            // If regex fails, fall back to simple equality check
            return path.Equals(normalizedPattern, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string GetVaultPath(DirectoryInfo vaultDir)
    {
        if (vaultDir != null)
            return vaultDir.FullName;

        var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
        if (!string.IsNullOrEmpty(convertConfig.obsidianVaultPath))
            return convertConfig.obsidianVaultPath;

        throw new InvalidOperationException(
            "No vault directory specified. Use --vault-dir option or configure vault path with 'obk config vault-dir'");
    }
}
