using System.Text.RegularExpressions;

namespace ObsidianKit.Utilities.Obsidian;

public class ConflictGroup
{
    public string originalFileName { get; set; }
    public string directoryPath { get; set; }
    public FileContentInfo originalFile { get; set; }
    public List<FileContentInfo> conflictFiles { get; set; } = new();
    public FileContentInfo largestFile { get; set; }
}

public class FileContentInfo
{
    public string filePath { get; set; }
    public bool isConflict { get; set; }
    public bool exists { get; set; }
    public int lineCount { get; set; }
    public long characterCount { get; set; }
}

public static class ObsidianConflictUtils
{
    private static readonly Regex ConflictPattern =
        new Regex(@" \(.+'s conflicted copy \d{4}-\d{2}-\d{2}\)", RegexOptions.IgnoreCase);

    public static List<string> FindConflictFiles(string vaultPath)
    {
        return Directory.EnumerateFiles(vaultPath, "*", SearchOption.AllDirectories)
            .Where(file => ConflictPattern.IsMatch(Path.GetFileName(file)))
            .ToList();
    }

    public static List<ConflictGroup> GroupConflictFiles(List<string> conflictFiles, string vaultPath)
    {
        var groups = new Dictionary<string, ConflictGroup>();

        foreach (var conflictFile in conflictFiles)
        {
            var originalFileName = GetOriginalFileName(Path.GetFileName(conflictFile));
            var directoryPath = Path.GetDirectoryName(conflictFile);
            var groupKey = Path.Combine(directoryPath, originalFileName);

            if (!groups.ContainsKey(groupKey))
            {
                var originalFilePath = Path.Combine(directoryPath, originalFileName);
                groups[groupKey] = new ConflictGroup
                {
                    originalFileName = originalFileName,
                    directoryPath = directoryPath,
                    originalFile = AnalyzeFileContent(originalFilePath, false)
                };
            }

            groups[groupKey].conflictFiles.Add(AnalyzeFileContent(conflictFile, true));
        }

        foreach (var group in groups.Values)
        {
            var allFiles = new List<FileContentInfo>();
            if (group.originalFile.exists)
                allFiles.Add(group.originalFile);
            allFiles.AddRange(group.conflictFiles);

            group.largestFile = allFiles
                .OrderByDescending(f => f.characterCount)
                .ThenByDescending(f => f.lineCount)
                .First();
        }

        return groups.Values.ToList();
    }

    public static FileContentInfo AnalyzeFileContent(string filePath, bool isConflict)
    {
        var info = new FileContentInfo
        {
            filePath = filePath,
            isConflict = isConflict,
            exists = File.Exists(filePath)
        };

        if (!info.exists)
            return info;

        try
        {
            var fileInfo = new FileInfo(filePath);
            info.characterCount = fileInfo.Length;

            if (IsProbablyTextFile(filePath))
            {
                info.lineCount = File.ReadLines(filePath).Count();
            }
        }
        catch
        {
            // If file cannot be read, keep default values
        }

        return info;
    }

    private static string GetOriginalFileName(string conflictFileName)
    {
        return ConflictPattern.Replace(conflictFileName, string.Empty);
    }

    private static bool IsProbablyTextFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var textExtensions = new[] { ".md", ".txt", ".json", ".xml", ".yaml", ".yml", ".css", ".js", ".ts", ".html", ".csv" };
        return textExtensions.Contains(extension);
    }
}
