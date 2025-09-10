using System.CommandLine;
using System.Linq;

namespace ObsidianKit;

public static class ConfigIgnoreCommand
{
    internal static Command CreateCommand()
    {
        var ignoreCommand = new Command("ignore", "Manage global ignored paths");
        ignoreCommand.AddCommand(CreateAddCommand());
        ignoreCommand.AddCommand(CreateRemoveCommand());
        ignoreCommand.AddCommand(CreateListCommand());
        return ignoreCommand;
    }

    private static Command CreateAddCommand()
    {
        var addCommand = new Command("add", "Add a path to the global ignore list");
        var pathArg = new Argument<string>("path", "Path to ignore globally");
        addCommand.AddArgument(pathArg);
        addCommand.SetHandler(AddIgnorePath, pathArg);
        return addCommand;

        void AddIgnorePath(string path)
        {
            var config = ConfigurationMgr.configuration;
            config.globalIgnoresPaths.Add(path);
            ConfigurationMgr.Save();
            Console.WriteLine($"Added '{path}' to global ignore list.");
        }
    }

    private static Command CreateRemoveCommand()
    {
        var removeCommand = new Command("remove", "Remove a path from the global ignore list");
        var pathArg = new Argument<string>("path", "Path to remove from global ignore list");
        removeCommand.AddArgument(pathArg);
        removeCommand.SetHandler(RemoveIgnorePath, pathArg);
        return removeCommand;

        void RemoveIgnorePath(string path)
        {
            var config = ConfigurationMgr.configuration;
            if (config.globalIgnoresPaths.Remove(path))
            {
                ConfigurationMgr.Save();
                Console.WriteLine($"Removed '{path}' from global ignore list.");
            }
            else
            {
                Console.WriteLine($"Path '{path}' was not found in the global ignore list.");
            }
        }
    }

    private static Command CreateListCommand()
    {
        var listCommand = new Command("list", "List all global ignored paths");
        listCommand.SetHandler(PrintAllIgnorePaths);
        return listCommand;

        void PrintAllIgnorePaths()
        {
            Console.WriteLine("Global Ignored Paths:");
            Console.WriteLine("(These paths are ignored across all operations)");
            var config = ConfigurationMgr.configuration;
            var ignorePaths = config.globalIgnoresPaths.ToList();
            if (ignorePaths.Any())
            {
                ignorePaths.Sort();
                for (int i = 0; i < ignorePaths.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {ignorePaths[i]}");
                }
            }
            else
            {
                Console.WriteLine("No paths are currently being ignored globally.");
            }
        }
    }
}
