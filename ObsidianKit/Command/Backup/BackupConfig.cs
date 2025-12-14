using System.Text.Json.Serialization;

namespace ObsidianKit;

public class BackupConfig : ICommandConfig
{
    [JsonIgnore]
    public string CommandName => "backup";

    public string backupDirectory { get; set; }

    public void SetDefaults()
    {
        if (string.IsNullOrEmpty(backupDirectory))
        {
            backupDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ObsidianBackups");
        }
    }

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        return true;
    }

    public void DisplayConfiguration()
    {
        Console.WriteLine("        Backup Configuration:");
        Console.WriteLine($"          Backup Directory: {backupDirectory}");
    }
}
