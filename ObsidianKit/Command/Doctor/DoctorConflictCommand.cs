using System.CommandLine;
using ObsidianKit.Utilities.Obsidian;

namespace ObsidianKit;

internal static class DoctorConflictCommand
{
    internal static Command CreateCommand()
    {
        var conflictCommand = new Command("conflict", "Detect and resolve conflict files in Obsidian vault");

        var vaultDirOption = new Option<DirectoryInfo>(
            "--vault-dir",
            "Path to Obsidian vault (uses configured path if not specified)");

        conflictCommand.AddOption(vaultDirOption);
        conflictCommand.SetHandler(HandleConflictDetection, vaultDirOption);

        return conflictCommand;
    }

    private static void HandleConflictDetection(DirectoryInfo vaultDir)
    {
        var vaultPath = GetVaultPath(vaultDir);

        Console.WriteLine($"Scanning Obsidian vault: {vaultPath}");
        Console.WriteLine("Checking all directories (including .obsidian, .trash, etc.)");
        Console.WriteLine("====================================");
        Console.WriteLine();

        var conflictFiles = ObsidianConflictUtils.FindConflictFiles(vaultPath);

        if (!conflictFiles.Any())
        {
            Console.WriteLine("No conflict files found.");
            return;
        }

        var conflictGroups = ObsidianConflictUtils.GroupConflictFiles(conflictFiles, vaultPath);

        DisplayConflictReport(conflictGroups);

        if (PromptForResolution())
        {
            ResolveConflictsInteractively(conflictGroups);
        }
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

    private static void DisplayConflictReport(List<ConflictGroup> groups)
    {
        Console.WriteLine($"Found {groups.Sum(g => g.conflictFiles.Count)} conflict files in {groups.Count} groups:");
        Console.WriteLine();

        foreach (var group in groups)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"Group: {Path.Combine(Path.GetFileName(group.directoryPath), group.originalFileName)}");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            if (group.originalFile.exists)
            {
                var marker = group.largestFile == group.originalFile ? " ⭐ LARGEST" : "";
                Console.WriteLine($"  ✓ {group.originalFileName} (Original){marker}");
                Console.WriteLine($"    Lines: {group.originalFile.lineCount:N0} | Characters: {group.originalFile.characterCount:N0}");
            }
            else
            {
                Console.WriteLine($"  ✗ {group.originalFileName} (Original - NOT FOUND)");
            }

            foreach (var conflict in group.conflictFiles)
            {
                var marker = group.largestFile == conflict ? " ⭐ LARGEST" : "";
                Console.WriteLine($"  • {Path.GetFileName(conflict.filePath)}{marker}");
                Console.WriteLine($"    Lines: {conflict.lineCount:N0} | Characters: {conflict.characterCount:N0}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Summary:");
        Console.WriteLine($"  Total conflict files: {groups.Sum(g => g.conflictFiles.Count)}");
        Console.WriteLine($"  Total groups: {groups.Count}");
        Console.WriteLine($"  Groups where original exists: {groups.Count(g => g.originalFile.exists)}");
        Console.WriteLine($"  Groups where original missing: {groups.Count(g => !g.originalFile.exists)}");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine();
    }

    private static bool PromptForResolution()
    {
        Console.Write("Do you want to automatically resolve conflicts? (y/N): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response == "y" || response == "yes";
    }

    private static void ResolveConflictsInteractively(List<ConflictGroup> groups)
    {
        Console.WriteLine();
        var resolvedCount = 0;
        var skippedCount = 0;
        var deletedFiles = 0;
        var renamedFiles = 0;

        foreach (var group in groups)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"Resolving: {Path.Combine(Path.GetFileName(group.directoryPath), group.originalFileName)}");
            Console.WriteLine($"Largest file: {Path.GetFileName(group.largestFile.filePath)} ({group.largestFile.characterCount:N0} characters)");
            Console.Write("Keep the largest file and delete others? (Y/n): ");

            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (response == "n" || response == "no")
            {
                Console.WriteLine("⊘ Skipped");
                Console.WriteLine();
                skippedCount++;
                continue;
            }

            try
            {
                var result = ResolveConflictGroup(group);
                deletedFiles += result.deletedCount;
                renamedFiles += result.renamedCount;
                resolvedCount++;
                Console.WriteLine("✓ Resolved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                skippedCount++;
            }

            Console.WriteLine();
        }

        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Resolution Summary:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"  Resolved groups: {resolvedCount}");
        Console.WriteLine($"  Skipped groups: {skippedCount}");
        Console.WriteLine($"  Files deleted: {deletedFiles}");
        Console.WriteLine($"  Files renamed: {renamedFiles}");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private static (int deletedCount, int renamedCount) ResolveConflictGroup(ConflictGroup group)
    {
        var deletedCount = 0;
        var renamedCount = 0;
        var originalFilePath = Path.Combine(group.directoryPath, group.originalFileName);

        if (group.largestFile.isConflict)
        {
            if (group.originalFile.exists)
            {
                File.Delete(originalFilePath);
                deletedCount++;
            }

            File.Move(group.largestFile.filePath, originalFilePath);
            renamedCount++;

            foreach (var conflict in group.conflictFiles.Where(c => c != group.largestFile))
            {
                File.Delete(conflict.filePath);
                deletedCount++;
            }
        }
        else
        {
            foreach (var conflict in group.conflictFiles)
            {
                File.Delete(conflict.filePath);
                deletedCount++;
            }
        }

        return (deletedCount, renamedCount);
    }
}
