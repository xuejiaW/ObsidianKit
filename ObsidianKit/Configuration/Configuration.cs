using System.Text.Json.Serialization;

namespace ObsidianKit;

public class Configuration
{
    [JsonIgnore]
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

    // Helper method to get convert config and its nested configs
    public ConvertConfig GetConvertConfig()
    {
        return GetCommandConfig<ConvertConfig>();
    }
}
