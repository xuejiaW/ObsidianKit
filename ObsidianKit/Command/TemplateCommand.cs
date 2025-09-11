using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

internal static class TemplateCommand
{
    internal static Command CreateCommand()
    {
        var templateCommand = new Command("template", "Manage and apply Obsidian vault templates");
        
        // Add subcommands
        templateCommand.AddCommand(CreateListCommand());
        templateCommand.AddCommand(CreateCreateCommand());
        templateCommand.AddCommand(CreateApplyCommand());
        templateCommand.AddCommand(CreateRemoveCommand());
        templateCommand.AddCommand(CreateSetDefaultVaultCommand());
        
        return templateCommand;
    }

    private static Command CreateListCommand()
    {
        var listCommand = new Command("list", "List all configured templates");
        listCommand.SetHandler(HandleListTemplates);
        return listCommand;
    }

    private static Command CreateCreateCommand()
    {
        var createCommand = new Command("create", "Create a new template from an Obsidian vault");
        
        var nameArg = new Argument<string>("name", "Name of the template");
        var sourceVaultArg = new Argument<DirectoryInfo?>("source-vault", 
            description: "Path to the source Obsidian vault", 
            getDefaultValue: () => {
                var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();
                var path = templateConfig.defaultVaultPath;
                return string.IsNullOrWhiteSpace(path) ? null : new DirectoryInfo(path);
            });
            
        createCommand.AddArgument(nameArg);
        createCommand.AddArgument(sourceVaultArg);
        createCommand.SetHandler(HandleCreateTemplate, nameArg, sourceVaultArg);
        
        return createCommand;
    }

    private static Command CreateApplyCommand()
    {
        var applyCommand = new Command("apply", "Apply a template to an Obsidian vault");
        
        var nameArg = new Argument<string>("name", "Name of the template to apply");
        var targetFolderArg = new Argument<DirectoryInfo?>("target-folder", 
            description: "Path to the target Obsidian vault",
            getDefaultValue: () => {
                var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();
                var path = templateConfig.defaultVaultPath;
                return string.IsNullOrWhiteSpace(path) ? null : new DirectoryInfo(path);
            });
            
        applyCommand.AddArgument(nameArg);
        applyCommand.AddArgument(targetFolderArg);
        applyCommand.SetHandler(HandleApplyTemplate, nameArg, targetFolderArg);
        
        return applyCommand;
    }

    private static Command CreateRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove a configured template");
        
        var nameArg = new Argument<string>("name", "Name of the template to remove");
        removeCommand.AddArgument(nameArg);
        removeCommand.SetHandler(HandleRemoveTemplate, nameArg);
        
        return removeCommand;
    }

    private static Command CreateSetDefaultVaultCommand()
    {
        var setDefaultVaultCommand = new Command("set-default-vault", "Set the default Obsidian vault path for template operations");
        
        var vaultPathArg = new Argument<DirectoryInfo>("vault-path", "Path to the default Obsidian vault");
        setDefaultVaultCommand.AddArgument(vaultPathArg);
        setDefaultVaultCommand.SetHandler(HandleSetDefaultVault, vaultPathArg);
        
        return setDefaultVaultCommand;
    }

    private static void HandleListTemplates()
    {
        var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();

        Console.WriteLine("Configured Templates:");
        Console.WriteLine("====================");

        if (!templateConfig.templates.Any())
        {
            Console.WriteLine("No templates configured.");
            Console.WriteLine("\nUse 'obsidiankit template create <name> [source-vault]' to create a new template.");
            return;
        }

        foreach (var template in templateConfig.templates)
        {
            var status = Directory.Exists(template.Value) ? "✓" : "✗ (path not found)";
            Console.WriteLine($"• {template.Key}: {template.Value} {status}");
        }
    }

    private static void HandleCreateTemplate(string name, DirectoryInfo? sourceVault)
    {
        if (sourceVault == null)
        {
            Console.WriteLine("Error: Source vault path is required.");
            Console.WriteLine("Usage: obsidiankit template create <name> <source-vault>");
            return;
        }

        FileSystemUtils.CheckDirectory(sourceVault, "Source Obsidian vault");

        // Check if .obsidian folder exists
        var obsidianConfigPath = Path.Combine(sourceVault.FullName, ".obsidian");
        if (!Directory.Exists(obsidianConfigPath))
        {
            Console.WriteLine($"Warning: No .obsidian folder found in {sourceVault.FullName}");
            Console.WriteLine("This might not be a valid Obsidian vault.");
        }

        var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();

        if (templateConfig.templates.ContainsKey(name))
        {
            Console.WriteLine($"Template '{name}' already exists. Do you want to overwrite it? (y/N)");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Template creation cancelled.");
                return;
            }
        }

        templateConfig.AddTemplate(name, sourceVault.FullName);
        ConfigurationMgr.SaveCommandConfig(templateConfig);

        Console.WriteLine($"Template '{name}' created successfully from '{sourceVault.FullName}'");
    }

    private static async Task HandleApplyTemplate(string name, DirectoryInfo? targetFolder)
    {
        if (targetFolder == null)
        {
            Console.WriteLine("Error: Target folder path is required.");
            Console.WriteLine("Usage: obsidiankit template apply <name> <target-folder>");
            return;
        }

        var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();

        var templatePath = templateConfig.GetTemplatePath(name);
        if (templatePath == null)
        {
            Console.WriteLine($"Template '{name}' not found.");
            Console.WriteLine("Use 'obsidiankit template list' to see available templates.");
            return;
        }

        if (!Directory.Exists(templatePath))
        {
            Console.WriteLine($"Template source path does not exist: {templatePath}");
            Console.WriteLine($"Use 'obsidiankit template remove {name}' to remove this invalid template.");
            return;
        }

        // Create target directory if it doesn't exist
        if (!targetFolder.Exists)
            targetFolder.Create();
        
        FileSystemUtils.CheckDirectory(targetFolder, "Target folder");

        try
        {
            await ApplyTemplateToVault(templatePath, targetFolder.FullName);
            Console.WriteLine($"Template '{name}' applied successfully to '{targetFolder.FullName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying template: {ex.Message}");
        }
    }

    private static void HandleRemoveTemplate(string name)
    {
        var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();

        if (!templateConfig.templates.ContainsKey(name))
        {
            Console.WriteLine($"Template '{name}' not found.");
            return;
        }

        templateConfig.RemoveTemplate(name);
        ConfigurationMgr.SaveCommandConfig(templateConfig);

        Console.WriteLine($"Template '{name}' removed successfully.");
    }

    private static void HandleSetDefaultVault(DirectoryInfo vaultPath)
    {
        FileSystemUtils.CheckDirectory(vaultPath, "Vault directory");

        var templateConfig = ConfigurationMgr.GetCommandConfig<TemplateConfig>();
        templateConfig.defaultVaultPath = vaultPath.FullName;
        ConfigurationMgr.SaveCommandConfig(templateConfig);

        Console.WriteLine($"Default vault path set to: {vaultPath.FullName}");
    }

    private static async Task ApplyTemplateToVault(string templatePath, string targetPath)
    {
        var obsidianConfigSource = Path.Combine(templatePath, ".obsidian");
        var pluginsSource = Path.Combine(templatePath, "Obsidian-Plugins");

        var obsidianConfigTarget = Path.Combine(targetPath, ".obsidian");
        var pluginsTarget = Path.Combine(targetPath, "Obsidian-Plugins");

        // Apply .obsidian configuration
        if (Directory.Exists(obsidianConfigSource))
        {
            Console.WriteLine("Copying .obsidian configuration...");
            
            // Backup existing configuration if it exists
            if (Directory.Exists(obsidianConfigTarget))
            {
                var backupPath = Path.Combine(targetPath, $".obsidian.backup.{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.Move(obsidianConfigTarget, backupPath);
                Console.WriteLine($"Existing .obsidian configuration backed up to: {backupPath}");
            }
            
            await FileSystemUtils.DeepCopyDirectory(new DirectoryInfo(obsidianConfigSource), obsidianConfigTarget);
        }

        // Apply Obsidian-Plugins
        if (Directory.Exists(pluginsSource))
        {
            Console.WriteLine("Copying Obsidian-Plugins...");
            
            // Backup existing plugins if they exist
            if (Directory.Exists(pluginsTarget))
            {
                var backupPath = Path.Combine(targetPath, $"Obsidian-Plugins.backup.{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.Move(pluginsTarget, backupPath);
                Console.WriteLine($"Existing Obsidian-Plugins backed up to: {backupPath}");
            }
            
            await FileSystemUtils.DeepCopyDirectory(new DirectoryInfo(pluginsSource), pluginsTarget);
        }
    }
}
