using System.CommandLine;
using ObsidianKit.Utilities;

namespace ObsidianKit;

public static class HexoConfigPostsDirCommand
{
    internal static Command CreateCommand()
    {
        var postsDirCmd = new Command("posts-dir", "Set Hexo posts directory");
        var hexoPostsDirArg = new Argument<DirectoryInfo>("directory", "Path to Hexo posts directory");
        postsDirCmd.AddArgument(hexoPostsDirArg);
        postsDirCmd.SetHandler(SetPostsDir, hexoPostsDirArg);
        return postsDirCmd;

        void SetPostsDir(DirectoryInfo hexoPostsDir)
        {
            FileSystemUtils.CheckDirectory(hexoPostsDir, "Hexo posts directory");

            var convertConfig = ConfigurationMgr.GetCommandConfig<ConvertConfig>();
            var config = convertConfig.GetCommandConfig<HexoConfig>();
            config.postsPath = hexoPostsDir.FullName;
            ConfigurationMgr.SaveCommandConfig(convertConfig);
            Console.WriteLine("Hexo posts directory has been set.");
        }
    }
}
