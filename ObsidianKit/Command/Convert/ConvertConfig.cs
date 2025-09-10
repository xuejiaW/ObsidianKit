namespace ObsidianKit;

public class ConvertConfig : ICommandConfig
{
    public string CommandName => "convert";
    
    public string obsidianVaultPath { get; set; }
    public HashSet<string> convertIgnoresPaths { get; set; } = new();
    
    // Nested command configs for convert subcommands
    public Dictionary<string, ICommandConfig> commandConfigs { get; } = new();

    public void AddCommandConfig<T>(T config) where T : class, ICommandConfig
    {
        commandConfigs[config.CommandName] = config;
    }

    public T GetCommandConfig<T>() where T : class, ICommandConfig, new()
    {
        var commandName = new T().CommandName;
        if (commandConfigs.TryGetValue(commandName, out var config))
        {
            return (T)config;
        }

        var newConfig = new T();
        newConfig.SetDefaults();
        commandConfigs[commandName] = newConfig;
        return newConfig;
    }

    public bool RemoveCommandConfig(string commandName)
    {
        return commandConfigs.Remove(commandName);
    }

    public IEnumerable<string> GetRegisteredCommands()
    {
        return commandConfigs.Keys;
    }

    public void SetDefaults()
    {
        if (string.IsNullOrEmpty(obsidianVaultPath))
            obsidianVaultPath = "";
            
        if (convertIgnoresPaths == null || !convertIgnoresPaths.Any())
            convertIgnoresPaths = new HashSet<string> { ".excalidraw.md" };
    }

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(obsidianVaultPath))
        {
            errors.Add("Obsidian vault path is not configured.");
        }
        else if (!Directory.Exists(obsidianVaultPath))
        {
            errors.Add($"Obsidian vault path does not exist: {obsidianVaultPath}");
        }

        return errors.Count == 0;
    }

    public void DisplayConfiguration()
    {
        Console.WriteLine("  Convert Command Configuration:");
        Console.WriteLine($"    Obsidian Vault Path: {obsidianVaultPath ?? "Not set"}");
        Console.WriteLine($"    Convert Ignored Paths: [{string.Join(", ", convertIgnoresPaths)}]");
        
        if (commandConfigs.Any())
        {
            Console.WriteLine("    Sub-commands:");
            foreach (var config in commandConfigs.Values)
            {
                Console.WriteLine($"      • {config.CommandName}");
                config.DisplayConfiguration();
            }
        }
    }
}
