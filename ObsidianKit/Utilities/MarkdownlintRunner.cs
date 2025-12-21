using System.Diagnostics;
using System.Text.Json;

namespace ObsidianKit.Utilities;

/// <summary>
/// Runs markdownlint-cli2 and parses the results
/// </summary>
public static class MarkdownlintRunner
{
    public class FormatIssue
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string RuleId { get; set; }
        public string RuleDescription { get; set; }
        public string ErrorDetail { get; set; }
        public string ErrorContext { get; set; }
    }
    
    public class FormatResult
    {
        public List<FormatIssue> Issues { get; set; } = new();
        public int TotalFiles { get; set; }
        public int FilesWithIssues { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Run markdownlint-cli2 on the specified path
    /// </summary>
    /// <param name="path">Path to check (file or directory)</param>
    /// <param name="configFile">Optional config file path</param>
    /// <param name="fix">Whether to automatically fix issues</param>
    /// <returns>Format result</returns>
    public static async Task<FormatResult> Run(string path, string configFile = null, bool fix = false)
    {
        var args = BuildArguments(path, configFile, fix);
        var commandName = MarkdownlintDetector.GetCommandName();
        
        // Set working directory to the vault path for proper relative path resolution
        var workingDir = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = commandName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir  // Set working directory
            }
        };
        
        try
        {
            process.Start();
            
            if (fix)
            {
                // Fix mode: show real-time output
                var outputTask = Task.Run(async () =>
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = await process.StandardOutput.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            Console.WriteLine(line);
                        }
                    }
                });
                
                var errorTask = Task.Run(async () =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = await process.StandardError.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line))
                        {
                            Console.Error.WriteLine(line);
                        }
                    }
                });
                
                await process.WaitForExitAsync();
                await Task.WhenAll(outputTask, errorTask);
                
                return new FormatResult
                {
                    Success = process.ExitCode <= 1,
                    Issues = new List<FormatIssue>(),
                    ErrorMessage = process.ExitCode > 1 ? $"markdownlint-cli2 exited with code {process.ExitCode}" : null
                };
            }
            else
            {
                // Check mode: capture stderr (markdownlint-cli2 writes there by default)
                Console.Write("Scanning files... ");
                
                // Start async reading before waiting for exit to avoid deadlock
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                var output = await outputTask;
                var error = await errorTask;
                
                Console.WriteLine("Done!");
                Console.WriteLine();
                
                // Exit code 0 = no issues, 1 = issues found, other = error
                if (process.ExitCode > 1)
                {
                    return new FormatResult
                    {
                        Success = false,
                        ErrorMessage = $"markdownlint-cli2 failed with exit code {process.ExitCode}: {error}"
                    };
                }
                
                // markdownlint-cli2 writes issues to stderr by default
                return ParseTextOutput(error, process.ExitCode == 0);
            }
        }
        catch (Exception ex)
        {
            return new FormatResult
            {
                Success = false,
                ErrorMessage = $"Failed to run markdownlint-cli2: {ex.Message}"
            };
        }
    }
    
    private static string BuildArguments(string path, string configFile, bool fix)
    {
        var args = new List<string>();
        
        // Fix mode
        if (fix)
        {
            args.Add("--fix");
        }
        
        // Config file
        if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
        {
            args.Add($"--config \"{configFile}\"");
        }
        
        // Target path - use glob pattern relative to the directory
        if (Directory.Exists(path))
        {
            // Use relative glob pattern since we set working directory
            // This allows config file ignores to work properly
            args.Add("\"**/*.md\"");
        }
        else
        {
            // For single files, use relative path
            var fileName = Path.GetFileName(path);
            args.Add($"\"{fileName}\"");
        }
        
        return string.Join(" ", args);
    }
    
    private static FormatResult ParseTextOutput(string output, bool noIssues)
    {
        if (noIssues || string.IsNullOrWhiteSpace(output))
        {
            return new FormatResult
            {
                Success = true,
                Issues = new List<FormatIssue>()
            };
        }
        
        try
        {
            var issues = new List<FormatIssue>();
            var filesSet = new HashSet<string>();
            
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                // Format: "path/file.md:36 error MD028/no-blanks-blockquote Blank line inside blockquote"
                // Or: "path/file.md:36:1 error MD028/no-blanks-blockquote Blank line inside blockquote"
                var match = System.Text.RegularExpressions.Regex.Match(
                    line, 
                    @"^(.+?):(\d+)(?::\d+)?\s+error\s+([A-Z0-9]+)[/\-]([a-z\-]+)\s+(.+)$"
                );
                
                if (match.Success)
                {
                    var filePath = match.Groups[1].Value;
                    var lineNumber = int.Parse(match.Groups[2].Value);
                    var ruleId = match.Groups[3].Value;
                    var ruleDescription = match.Groups[5].Value;
                    
                    filesSet.Add(filePath);
                    
                    issues.Add(new FormatIssue
                    {
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        RuleId = ruleId,
                        RuleDescription = ruleDescription,
                        ErrorDetail = null,
                        ErrorContext = null
                    });
                }
            }
            
            return new FormatResult
            {
                Success = true,
                Issues = issues,
                TotalFiles = filesSet.Count,
                FilesWithIssues = filesSet.Count
            };
        }
        catch (Exception ex)
        {
            return new FormatResult
            {
                Success = false,
                ErrorMessage = $"Failed to parse markdownlint output: {ex.Message}"
            };
        }
    }
}
