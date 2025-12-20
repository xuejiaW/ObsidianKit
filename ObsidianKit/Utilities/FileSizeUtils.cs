using System.Text.RegularExpressions;

namespace ObsidianKit.Utilities;

public static class FileSizeUtils
{
    /// <summary>
    /// Format file size in bytes to human-readable format (B, KB, MB, GB)
    /// </summary>
    /// <param name="bytes">Size in bytes</param>
    /// <returns>Formatted size string</returns>
    public static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
    
    /// <summary>
    /// Parse size string (e.g., "1.5MB", "500KB") to bytes
    /// </summary>
    /// <param name="sizeStr">Size string to parse</param>
    /// <returns>Size in bytes</returns>
    /// <exception cref="ArgumentException">Thrown when size format is invalid</exception>
    public static long ParseSize(string sizeStr)
    {
        sizeStr = sizeStr.Trim().ToUpper();
        var match = Regex.Match(sizeStr, @"^(\d+(?:\.\d+)?)\s*(B|KB|MB|GB)?$");
        
        if (!match.Success)
            throw new ArgumentException($"Invalid size format: {sizeStr}. Use format like '1.5MB' or '500KB'");
        
        var value = double.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value;
        
        return unit switch
        {
            "GB" => (long)(value * 1024 * 1024 * 1024),
            "MB" => (long)(value * 1024 * 1024),
            "KB" => (long)(value * 1024),
            _ => (long)value
        };
    }
}
