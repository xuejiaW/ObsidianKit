using System.Reflection;
using System.Text.Json;

namespace ObsidianKit;

public static class ConfigurationMgr
{
    private static string s_ConfigurationPath = null;

    public static string configurationPath => s_ConfigurationPath ??= Path.Join(configurationFolder, "configuration.json");

    private static readonly Dictionary<string, Type> RegisteredTypes = GetCommandConfigTypes();

    private static string configurationFolder
    {
        get
        {
            var homeVar = Environment.OSVersion.Platform == PlatformID.Unix ? "HOME" : "USERPROFILE";
            var baseFolder = Environment.OSVersion.Platform == PlatformID.Unix ? ".config" : "AppData/Local";
            return Path.Combine(Environment.GetEnvironmentVariable(homeVar) ?? "", baseFolder, "ObsidianKit");
        }
    }

    private static Configuration s_Configuration;

    public static Configuration configuration
    {
        get
        {
            if (s_Configuration == null)
            {
                if (!Directory.Exists(configurationFolder))
                    Directory.CreateDirectory(configurationFolder);

                if (!File.Exists(configurationPath))
                {
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ObsidianKit.Resources.configuration.json");
                    using var fileStream = File.Create(configurationPath);
                    stream?.CopyTo(fileStream);
                    Console.WriteLine($"Created initial configuration at: {configurationPath}");

                }

                s_Configuration = Load();
            }
            return s_Configuration;
        }
        private set => s_Configuration = value;

    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static Dictionary<string, Type> GetCommandConfigTypes()
    {
        var types = new Dictionary<string, Type>();
        var assembly = Assembly.GetExecutingAssembly();
        
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && typeof(ICommandConfig).IsAssignableFrom(type))
            {
                try
                {
                    var instance = (ICommandConfig)Activator.CreateInstance(type);
                    types[instance.CommandName] = type;
                }
                catch
                {
                    // Skip types that can't be instantiated
                }
            }
        }
        
        return types;
    }

    public static Configuration Load()
    {
        try
        {
            var jsonString = File.ReadAllText(configurationPath);

            using var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            var config = new Configuration();

            // Load global ignore paths
            if (root.TryGetProperty("globalIgnoresPaths", out var globalIgnores))
            {
                config.globalIgnoresPaths = globalIgnores.Deserialize<HashSet<string>>(JsonOptions) ?? new HashSet<string>();
            }

            // Load command configurations
            if (root.TryGetProperty("commandConfigs", out var commandConfigs))
            {
                LoadCommandConfigs(commandConfigs, config.commandConfigs);
            }

            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return new Configuration();
        }
    }

    private static void LoadCommandConfigs(JsonElement commandConfigs, Dictionary<string, ICommandConfig> targetConfigs)
    {
        foreach (var commandProp in commandConfigs.EnumerateObject())
        {
            var commandName = commandProp.Name;
            var commandJson = commandProp.Value;

            if (!commandJson.TryGetProperty("$type", out var typeElement))
                continue;

            var typeName = typeElement.GetString();
            var configType = RegisteredTypes.Values.FirstOrDefault(t => t.Name == typeName);
            if (configType == null)
                continue;

            // Create a new JSON object without the $type property
            var configData = new Dictionary<string, object>();
            foreach (var prop in commandJson.EnumerateObject())
            {
                if (prop.Name != "$type") 
                    configData[prop.Name] = prop.Value;
            }

            var cleanJsonText = JsonSerializer.Serialize(configData, JsonOptions);
            var commandConfig = (ICommandConfig)JsonSerializer.Deserialize(cleanJsonText, configType, JsonOptions);
            if (commandConfig == null)
                continue;

            commandConfig.SetDefaults();
            
            // Handle nested command configs for ConvertConfig
            if (commandConfig is ConvertConfig convertConfig && 
                commandJson.TryGetProperty("commandConfigs", out var nestedConfigs))
            {
                LoadCommandConfigs(nestedConfigs, convertConfig.commandConfigs);
            }
            
            targetConfigs[commandName] = commandConfig;
        }
    }

    public static void Save()
    {
        try
        {
            var commandConfigsWithTypes = SerializeCommandConfigs(configuration.commandConfigs);

            var configToSave = new
            {
                globalIgnoresPaths = configuration.globalIgnoresPaths,
                commandConfigs = commandConfigsWithTypes
            };

            var json = JsonSerializer.Serialize(configToSave, JsonOptions);
            File.WriteAllText(configurationPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }

    private static Dictionary<string, object> SerializeCommandConfigs(Dictionary<string, ICommandConfig> commandConfigs)
    {
        var commandConfigsWithTypes = new Dictionary<string, object>();
        
        foreach (var kvp in commandConfigs)
        {
            var configType = kvp.Value.GetType();
            var configData = JsonSerializer.SerializeToElement(kvp.Value, configType, JsonOptions);

            var configWithType = new Dictionary<string, object>();
            configWithType["$type"] = configType.Name;

            // Add all properties from the original config
            foreach (var prop in configData.EnumerateObject())
            {
                // Handle nested command configs for ConvertConfig
                if (prop.Name == "commandConfigs" && kvp.Value is ConvertConfig convertConfig)
                {
                    configWithType[prop.Name] = SerializeCommandConfigs(convertConfig.commandConfigs);
                }
                else
                {
                    configWithType[prop.Name] = prop.Value;
                }
            }

            commandConfigsWithTypes[kvp.Key] = configWithType;
        }

        return commandConfigsWithTypes;
    }

    public static void UpdateConfiguration(Configuration newConfiguration)
    {
        configuration = newConfiguration;
        Save();
    }

    public static T GetCommandConfig<T>() where T : class, ICommandConfig, new()
    {
        return configuration.GetCommandConfig<T>();
    }

    public static void SaveCommandConfig<T>(T config) where T : class, ICommandConfig
    {
        configuration.AddCommandConfig(config);
        Save();
    }
}
