using System.Text.Json.Serialization;

namespace ObsidianKit;

public class DoctorFormatConfig : ICommandConfig
{
    [JsonIgnore]
    public string CommandName => "doctor-format";

    [JsonPropertyName("ignorePatterns")]
    public HashSet<string> ignorePatterns { get; set; } = new();

    public void SetDefaults()
    {
        if (ignorePatterns == null || !ignorePatterns.Any())
        {
            ignorePatterns = new HashSet<string>
            {
                "node_modules/**",
                ".git/**",
                ".obsidian/**",
                ".trash/**"
            };
        }
    }

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        
        // No validation needed for now
        return true;
    }

    public void DisplayConfiguration()
    {
        Console.WriteLine("Doctor Format Configuration:");
        
        Console.WriteLine($"  Ignore Patterns ({ignorePatterns.Count}):");
        if (ignorePatterns.Any())
        {
            foreach (var pattern in ignorePatterns.OrderBy(p => p))
            {
                Console.WriteLine($"    - {pattern}");
            }
        }
        else
        {
            Console.WriteLine("    (none)");
        }
    }
}
