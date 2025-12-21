using System.Text.RegularExpressions;

namespace ObsidianKit.Utilities;

/// <summary>
/// Helper to sync ObsidianKit ignore patterns to .markdownlint-cli2.jsonc config file
/// </summary>
public static class MarkdownlintConfigSync
{
    private const string BeginMarker = "// BEGIN OBSIDIANKIT IGNORES";
    private const string EndMarker = "// END OBSIDIANKIT IGNORES";
    
    /// <summary>
    /// Sync ignore patterns to vault's .markdownlint-cli2.jsonc config file
    /// </summary>
    /// <param name="vaultPath">Path to vault directory</param>
    /// <param name="showMessage">Whether to show sync success message</param>
    /// <returns>True if synced successfully, false otherwise</returns>
    public static bool SyncIgnorePatternsToConfigFile(string vaultPath, bool showMessage = false)
    {
        var configPath = Path.Combine(vaultPath, ".markdownlint-cli2.jsonc");
        
        // Only sync if user config exists
        if (!File.Exists(configPath))
            return false;
        
        var formatConfig = ConfigurationMgr.GetCommandConfig<DoctorFormatConfig>();
        
        // If no ObsidianKit ignore patterns, nothing to sync
        if (!formatConfig.ignorePatterns.Any())
            return false;
        
        try
        {
            var content = File.ReadAllText(configPath);
            
            // Check if markers exist
            if (!content.Contains(BeginMarker) || !content.Contains(EndMarker))
            {
                if (showMessage)
                {
                    ShowMarkerInstructions();
                }
                return false;
            }
            
            // Generate ignore patterns section
            var newIgnoresSection = GenerateIgnoresSection(formatConfig.ignorePatterns);
            
            // Replace the section between markers
            var regexPattern = @"// BEGIN OBSIDIANKIT IGNORES.*?// END OBSIDIANKIT IGNORES";
            var newContent = Regex.Replace(
                content,
                regexPattern,
                newIgnoresSection,
                RegexOptions.Singleline
            );
            
            // Only write if content changed
            if (newContent != content)
            {
                File.WriteAllText(configPath, newContent);
                
                if (showMessage)
                {
                    Console.WriteLine($"✓ Synced to {Path.GetFileName(configPath)}");
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            // Don't fail the whole operation if sync fails
            if (showMessage)
            {
                Console.WriteLine($"Warning: Failed to sync ignore patterns: {ex.Message}");
            }
            return false;
        }
    }
    
    private static string GenerateIgnoresSection(HashSet<string> ignorePatterns)
    {
        var patterns = ignorePatterns.OrderBy(p => p).ToList();
        var ignoreLines = new List<string>();
        
        if (patterns.Any())
        {
            for (int i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i].Replace('\\', '/');
                var comma = i < patterns.Count - 1 ? "," : "";
                ignoreLines.Add($"    \"{pattern}\"{comma}");
            }
        }
        
        return BeginMarker + "\n" + 
               (ignoreLines.Any() ? string.Join("\n", ignoreLines) + "\n    " : "    ") + 
               EndMarker;
    }
    
    private static void ShowMarkerInstructions()
    {
        Console.WriteLine();
        Console.WriteLine("Note: To enable automatic sync to .markdownlint-cli2.jsonc,");
        Console.WriteLine("      please add the following markers to the 'ignores' array:");
        Console.WriteLine();
        Console.WriteLine("  \"ignores\": [");
        Console.WriteLine("    \"node_modules/**\",");
        Console.WriteLine("    \".git/**\",");
        Console.WriteLine($"    {BeginMarker}");
        Console.WriteLine($"    {EndMarker}");
        Console.WriteLine("  ]");
        Console.WriteLine();
        Console.WriteLine("  Changes are saved to ObsidianKit config only.");
    }
}
