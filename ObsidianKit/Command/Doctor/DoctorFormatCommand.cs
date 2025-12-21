using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using ObsidianKit.ConsoleUI;
using ObsidianKit.Utilities;

namespace ObsidianKit;

internal static class DoctorFormatCommand
{
    internal static Command CreateCommand()
    {
        var formatCommand = new Command("format", "Check and fix Markdown formatting issues using markdownlint-cli2");

        var vaultDirOption = new Option<DirectoryInfo>(
            "--vault-dir",
            "Path to Obsidian vault (uses configured path if not specified)");

        var fixOption = new Option<bool>(
            "--fix",
            "Automatically fix formatting issues");

        var verboseOption = new Option<bool>(
            "--verbose",
            "Show detailed list of all issues");

        var rulesOption = new Option<string>(
            "--rules",
            "Only check/fix specific rules (comma-separated, e.g., MD030,MD031)");

        var configOption = new Option<FileInfo>(
            "--config",
            "Path to markdownlint config file (default: .markdownlint-cli2.jsonc or .markdownlint.json in vault root)");

        formatCommand.AddOption(vaultDirOption);
        formatCommand.AddOption(fixOption);
        formatCommand.AddOption(verboseOption);
        formatCommand.AddOption(rulesOption);
        formatCommand.AddOption(configOption);
        formatCommand.AddCommand(DoctorFormatConfigCommand.CreateCommand());
        formatCommand.SetHandler(HandleFormat, vaultDirOption, fixOption, verboseOption, rulesOption, configOption);

        return formatCommand;
    }

    private static async Task HandleFormat(DirectoryInfo vaultDir, bool fix, bool verbose, string rules, FileInfo configFile)
    {
        // Check if markdownlint-cli2 is available
        if (!MarkdownlintDetector.IsAvailable())
        {
            Console.WriteLine(MarkdownlintDetector.GetInstallInstructions());
            return;
        }

        // Get vault path
        var vaultPath = GetVaultPath(vaultDir);

        // Display version
        var version = MarkdownlintDetector.GetVersion();
        Console.WriteLine($"Using markdownlint-cli2 {version}");
        Console.WriteLine($"Checking Markdown files in: {vaultPath}");
        Console.WriteLine();

        // Sync ObsidianKit ignore patterns to config file
        MarkdownlintConfigSync.SyncIgnorePatternsToConfigFile(vaultPath);

        // Handle rules filter
        string tempConfigPath = null;
        string renamedConfigPath = null;
        if (!string.IsNullOrEmpty(rules))
        {
            // Find main config first
            var mainConfig = FindConfigFile(vaultPath);
            
            // Create temp config BEFORE renaming (so we can read it)
            tempConfigPath = CreateTempConfigForRules(vaultPath, rules, mainConfig);
            
            // Now rename the main config to prevent it from being loaded
            if (mainConfig != null && File.Exists(mainConfig))
            {
                renamedConfigPath = mainConfig + ".obk-backup";
                File.Move(mainConfig, renamedConfigPath);
            }
            
            configFile = new FileInfo(tempConfigPath);
        }

        // Find config file
        string configPath = null;
        
        if (configFile != null && configFile.Exists)
        {
            configPath = configFile.FullName;
            Console.WriteLine($"Using config: {configPath}");
        }
        else
        {
            configPath = FindConfigFile(vaultPath);
            if (configPath != null)
            {
                Console.WriteLine($"Using config: {Path.GetFileName(configPath)}");
            }
            else
            {
                Console.WriteLine("No config file found, using default rules");
            }
        }

        Console.WriteLine();

        if (fix)
        {
            Console.WriteLine("Running with --fix (will modify files)...");
        }
        else
        {
            Console.WriteLine("Running format check...");
        }

        Console.WriteLine();

        // Run markdownlint
        var result = await MarkdownlintRunner.Run(vaultPath, configPath, fix);

        // Clean up temp config and restore main config
        if (tempConfigPath != null && File.Exists(tempConfigPath))
        {
            try { File.Delete(tempConfigPath); } catch { }
        }
        if (renamedConfigPath != null && File.Exists(renamedConfigPath))
        {
            var mainConfig = renamedConfigPath.Replace(".obk-backup", "");
            try { File.Move(renamedConfigPath, mainConfig); } catch { }
        }

        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
            return;
        }

        // Display results
        if (fix)
        {
            Console.WriteLine();
            Console.WriteLine("✓ Format check and fix completed!");
        }
        else
        {
            Console.WriteLine();
            DisplayResultsWithStatistics(result, vaultPath, verbose);
        }
    }

    private static void DisplayResultsWithStatistics(MarkdownlintRunner.FormatResult result, string vaultPath, bool verbose)
    {
        if (!result.Issues.Any())
        {
            Console.WriteLine("✓ No formatting issues found!");
            return;
        }

        Console.WriteLine($"Found {result.Issues.Count} issue(s) in {result.FilesWithIssues} file(s)");
        Console.WriteLine();

        // Statistics by rule - group by RuleId only
        var ruleStats = result.Issues
            .GroupBy(i => i.RuleId)
            .Select(g => new
            {
                RuleId = g.Key,
                RuleDescription = g.First().RuleDescription, // Take first description
                Count = g.Count(),
                Percentage = (double)g.Count() / result.Issues.Count * 100
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        Console.WriteLine("📊 Issues by Rule:");
        Console.WriteLine();
        
        foreach (var stat in ruleStats)
        {
            var bar = new string('█', (int)(stat.Percentage / 2)); // Scale to max 50 chars
            Console.WriteLine($"  {stat.RuleId,-8} {stat.Count,4} ({stat.Percentage,5:F1}%) {bar}");
            Console.WriteLine($"           {stat.RuleDescription}");
        }
        
        Console.WriteLine();
        Console.WriteLine("📁 Affected Files:");
        Console.WriteLine();

        // Group by file
        var issuesByFile = result.Issues
            .GroupBy(i => i.FilePath)
            .OrderByDescending(g => g.Count())
            .Take(20); // Show top 20 files

        var lines = new List<string>();
        
        foreach (var fileGroup in issuesByFile)
        {
            // Get relative path from vault root
            var filePath = fileGroup.Key;
            string relativePath;
            
            // Ensure both paths use the same separator and casing for comparison
            var normalizedFilePath = Path.GetFullPath(filePath).Replace('\\', '/').ToLowerInvariant();
            var normalizedVaultPath = Path.GetFullPath(vaultPath).Replace('\\', '/').ToLowerInvariant();
            
            // Check if file is under vault directory
            if (normalizedFilePath.StartsWith(normalizedVaultPath + "/") || 
                normalizedFilePath.StartsWith(normalizedVaultPath))
            {
                // Get relative path
                relativePath = filePath.Substring(vaultPath.Length).TrimStart('\\', '/');
            }
            else
            {
                // File is outside vault, use absolute path
                relativePath = filePath;
            }
            
            lines.Add($"{fileGroup.Count(),3} issue(s): {relativePath}");
        }

        if (result.FilesWithIssues > 20)
        {
            lines.Add("");
            lines.Add($"... and {result.FilesWithIssues - 20} more files");
        }

        Pager.DisplayWithPagination(lines);

        Console.WriteLine();
        Console.WriteLine("💡 Tips:");
        Console.WriteLine("  • To fix issues automatically: obk doctor format --fix");
        Console.WriteLine("  • To see details for a specific file: markdownlint-cli2 <file>");
        Console.WriteLine("  • To learn about rules: https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md");
        
        // Verbose mode: show all issues
        if (verbose)
        {
            Console.WriteLine();
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("📋 Detailed Issues List:");
            Console.WriteLine();
            
            var allIssuesByFile = result.Issues
                .GroupBy(i => i.FilePath)
                .OrderBy(g => g.Key);
            
            foreach (var fileGroup in allIssuesByFile)
            {
                // Get relative path from vault root
                var filePath = fileGroup.Key;
                string relativePath;
                
                // Ensure both paths use the same separator and casing for comparison
                var normalizedFilePath = Path.GetFullPath(filePath).Replace('\\', '/').ToLowerInvariant();
                var normalizedVaultPath = Path.GetFullPath(vaultPath).Replace('\\', '/').ToLowerInvariant();
                
                // Check if file is under vault directory
                if (normalizedFilePath.StartsWith(normalizedVaultPath + "/") || 
                    normalizedFilePath.StartsWith(normalizedVaultPath))
                {
                    // Get relative path
                    relativePath = filePath.Substring(vaultPath.Length).TrimStart('\\', '/');
                }
                else
                {
                    // File is outside vault, use absolute path
                    relativePath = filePath;
                }
                
                foreach (var issue in fileGroup.OrderBy(i => i.LineNumber))
                {
                    // Format: filename:line [RuleId] Description
                    var line = $"{relativePath}:{issue.LineNumber} [{issue.RuleId}] {issue.RuleDescription}";
                    
                    if (!string.IsNullOrEmpty(issue.ErrorDetail))
                    {
                        line += $" - {issue.ErrorDetail}";
                    }
                    
                    Console.WriteLine(line);
                }
            }
        }
    }

    private static string CreateTempConfigForRules(string vaultPath, string rulesStr, string baseConfigPath)
    {
        var rulesList = rulesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // Use supported config file name format
        var tempConfigPath = Path.Combine(vaultPath, ".obk-temp.markdownlint-cli2.jsonc");
        
        // Find base config if not specified
        if (string.IsNullOrEmpty(baseConfigPath))
        {
            baseConfigPath = FindConfigFile(vaultPath);
        }
        
        JsonNode configNode;
        
        // Try to read and parse base config
        if (!string.IsNullOrEmpty(baseConfigPath) && File.Exists(baseConfigPath))
        {
            try
            {
                var baseConfigText = File.ReadAllText(baseConfigPath);
                
                // Remove comments for JSON parsing (simple approach for JSONC)
                var lines = baseConfigText.Split('\n');
                var cleanedLines = lines.Select(line =>
                {
                    var commentIndex = line.IndexOf("//");
                    return commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
                });
                var cleanedJson = string.Join('\n', cleanedLines);
                
                configNode = JsonNode.Parse(cleanedJson);
            }
            catch
            {
                // If parsing fails, create new config
                configNode = new JsonObject();
            }
        }
        else
        {
            configNode = new JsonObject();
        }
        
        // Modify config to only enable specified rules
        var configSection = new JsonObject
        {
            ["default"] = false
        };
        
        foreach (var rule in rulesList)
        {
            configSection[rule.Trim()] = true;
        }
        
        configNode["config"] = configSection;
        
        // Write temp config
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        File.WriteAllText(tempConfigPath, configNode.ToJsonString(options));
        
        Console.WriteLine($"Filtering rules: {string.Join(", ", rulesList)}");
        Console.WriteLine();
        
        return tempConfigPath;
    }

    private static string FindConfigFile(string vaultPath)
    {
        var configFiles = new[]
        {
            ".markdownlint-cli2.jsonc",
            ".markdownlint-cli2.yaml",
            ".markdownlint-cli2.json",
            ".markdownlint.jsonc",
            ".markdownlint.json",
            ".markdownlint.yaml"
        };

        foreach (var file in configFiles)
        {
            var path = Path.Combine(vaultPath, file);
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static string GetVaultPath(DirectoryInfo vaultDir)
    {
        if (vaultDir != null && vaultDir.Exists)
            return vaultDir.FullName;

        var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
        if (!string.IsNullOrEmpty(convertConfig.obsidianVaultPath))
            return convertConfig.obsidianVaultPath;

        throw new InvalidOperationException(
            "No vault directory specified. Use --vault-dir option or configure vault path with 'obk config vault-dir'");
    }
}
