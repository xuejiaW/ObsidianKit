using System.CommandLine;

namespace ObsidianKit
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(ConvertCommand.CreateCommand());
            rootCommand.AddCommand(ConfigCommand.CreateCommand());

            rootCommand.SetHandler(() => { });

            return await rootCommand.InvokeAsync(args);
        }
    }
}
