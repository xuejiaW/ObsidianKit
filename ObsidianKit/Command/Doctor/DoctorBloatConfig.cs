using System.Text.Json.Serialization;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public class DoctorBloatConfig : ICommandConfig
{
    [JsonIgnore]
    public string CommandName => "doctor-bloat";
    
    /// <summary>
    /// File path patterns to ignore (supports wildcards)
    /// </summary>
    public HashSet<string> ignorePatterns { get; set; } = new();
    
    /// <summary>
    /// File size limits by extension (in bytes)
    /// </summary>
    public Dictionary<string, long> fileSizeLimits { get; set; } = new();
    
    /// <summary>
    /// Default maximum size for unspecified file types (in bytes)
    /// </summary>
    public long defaultMaxSize { get; set; }
    
    public void SetDefaults()
    {
        if (ignorePatterns == null || !ignorePatterns.Any())
        {
            ignorePatterns = new HashSet<string>();
        }
        
        if (fileSizeLimits == null || !fileSizeLimits.Any())
        {
            fileSizeLimits = new Dictionary<string, long>
            {
                // Image files - 1MB
                { ".png", 1 * 1024 * 1024 },
                { ".jpg", 1 * 1024 * 1024 },
                { ".jpeg", 1 * 1024 * 1024 },
                { ".gif", 1 * 1024 * 1024 },
                { ".webp", 1 * 1024 * 1024 },
                
                // Video files - 10MB
                { ".mp4", 10 * 1024 * 1024 },
                { ".mov", 10 * 1024 * 1024 },
                { ".avi", 10 * 1024 * 1024 },
                
                // Audio files - 5MB
                { ".mp3", 5 * 1024 * 1024 },
                { ".wav", 5 * 1024 * 1024 },
                { ".m4a", 5 * 1024 * 1024 },
                
                // Document files - 2MB
                { ".pdf", 2 * 1024 * 1024 },
                { ".docx", 2 * 1024 * 1024 },
                
                // Markdown - 500KB
                { ".md", 500 * 1024 }
            };
        }
        
        if (defaultMaxSize == 0)
        {
            defaultMaxSize = 2 * 1024 * 1024; // Default 2MB
        }
    }
    
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        
        if (defaultMaxSize <= 0)
        {
            errors.Add("defaultMaxSize must be greater than 0");
        }
        
        foreach (var (ext, size) in fileSizeLimits)
        {
            if (size <= 0)
            {
                errors.Add($"Size limit for '{ext}' must be greater than 0");
            }
            
            if (!ext.StartsWith("."))
            {
                errors.Add($"File extension '{ext}' must start with a dot (e.g., '.png')");
            }
        }
        
        return !errors.Any();
    }
    
    public void DisplayConfiguration()
    {
        Console.WriteLine("    Doctor Bloat Configuration:");
        Console.WriteLine($"      Default Max Size: {FileSizeUtils.FormatSize(defaultMaxSize)}");
        
        Console.WriteLine($"      Ignore Patterns ({ignorePatterns.Count}):");
        if (ignorePatterns.Any())
        {
            foreach (var pattern in ignorePatterns.OrderBy(p => p))
            {
                Console.WriteLine($"        - {pattern}");
            }
        }
        else
        {
            Console.WriteLine("        (none)");
        }
        
        Console.WriteLine($"      File Size Limits ({fileSizeLimits.Count}):");
        foreach (var (ext, size) in fileSizeLimits.OrderBy(x => x.Key))
        {
            Console.WriteLine($"        {ext,-10} : {FileSizeUtils.FormatSize(size)}");
        }
    }
}
