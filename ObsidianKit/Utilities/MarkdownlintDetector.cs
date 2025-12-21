using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ObsidianKit.Utilities;

/// <summary>
/// Detects if markdownlint-cli2 is installed and provides installation instructions
/// </summary>
public static class MarkdownlintDetector
{
    /// <summary>
    /// Get the correct command name for the current platform
    /// </summary>
    /// <returns>Command name to use</returns>
    public static string GetCommandName()
    {
        var commands = new[] { "markdownlint-cli2" };
        
        // On Windows, also try .cmd extension
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            commands = new[] { "markdownlint-cli2.cmd", "markdownlint-cli2" };
        }
        
        foreach (var command in commands)
        {
            if (TryCommand(command))
                return command;
        }
        
        // Return default if none work
        return "markdownlint-cli2";
    }
    /// <summary>
    /// Check if markdownlint-cli2 is available on the system
    /// </summary>
    /// <returns>True if markdownlint-cli2 is found, false otherwise</returns>
    public static bool IsAvailable()
    {
        // Try different command names
        var commands = new[] { "markdownlint-cli2" };
        
        // On Windows, also try .cmd extension
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            commands = new[] { "markdownlint-cli2", "markdownlint-cli2.cmd" };
        }
        
        foreach (var command in commands)
        {
            if (TryCommand(command))
                return true;
        }
        
        return false;
    }
    
    private static bool TryCommand(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "",  // No arguments - will show help
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            // Check if output contains markdownlint-cli2
            var combinedOutput = output + error;
            return combinedOutput.Contains("markdownlint-cli2");
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Get the installed version of markdownlint-cli2
    /// </summary>
    /// <returns>Version string, or null if not installed</returns>
    public static string GetVersion()
    {
        var commands = new[] { "markdownlint-cli2" };
        
        // On Windows, also try .cmd extension
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            commands = new[] { "markdownlint-cli2", "markdownlint-cli2.cmd" };
        }
        
        foreach (var command in commands)
        {
            var version = TryGetVersion(command);
            if (version != null)
                return version;
        }
        
        return null;
    }
    
    private static string TryGetVersion(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "",  // No arguments - will show help with version
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            // Version info is in the first line of output
            var combinedOutput = output + error;
            var lines = combinedOutput.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains("markdownlint-cli2") && line.Contains("v"))
                {
                    // Extract version from line like "markdownlint-cli2 v0.20.0 (markdownlint v0.40.0)"
                    return line.Trim();
                }
            }
        }
        catch
        {
            // Ignore
        }
        
        return null;
    }
    
    /// <summary>
    /// Get platform-specific installation instructions
    /// </summary>
    /// <returns>Installation instructions string</returns>
    public static string GetInstallInstructions()
    {
        var instructions = @"markdownlint-cli2 is required but not found.

To install markdownlint-cli2, run:
  npm install -g markdownlint-cli2

";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            instructions += @"If you don't have Node.js installed:
  1. Download from: https://nodejs.org/
  2. Or use winget: winget install OpenJS.NodeJS
  3. Or use Chocolatey: choco install nodejs

After installing Node.js, run:
  npm install -g markdownlint-cli2";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            instructions += @"If you don't have Node.js installed:
  1. Download from: https://nodejs.org/
  2. Or use Homebrew: brew install node

After installing Node.js, run:
  npm install -g markdownlint-cli2";
        }
        else // Linux
        {
            instructions += @"If you don't have Node.js installed:
  1. Download from: https://nodejs.org/
  2. Or use package manager:
     - Ubuntu/Debian: sudo apt install nodejs npm
     - Fedora: sudo dnf install nodejs npm
     - Arch: sudo pacman -S nodejs npm

After installing Node.js, run:
  npm install -g markdownlint-cli2";
        }
        
        instructions += @"

For more information:
  https://github.com/DavidAnson/markdownlint-cli2";
        
        return instructions;
    }
}
