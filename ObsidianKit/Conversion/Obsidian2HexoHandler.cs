using ObsidianKit.ConsoleUI;
using ObsidianKit.Utilities;
using ObsidianKit.Utilities.Obsidian;
using ObsidianKit.Utilities.Hexo;
using Progress = ObsidianKit.ConsoleUI.Progress;

namespace ObsidianKit;

internal class ObsidianKitHexoHandler
{
    public static DirectoryInfo obsidianVaultDir { get; private set; }
    public static DirectoryInfo hexoPostsDir { get; private set; }
    public static DirectoryInfo obsidianTempDir { get; private set; }

    public ObsidianKitHexoHandler(DirectoryInfo obsidianVaultDir, DirectoryInfo hexoPostsDir)
    {
        ObsidianKitHexoHandler.obsidianVaultDir = obsidianVaultDir;
        ObsidianKitHexoHandler.hexoPostsDir = hexoPostsDir;
        obsidianTempDir = new DirectoryInfo(ObsidianKitHexoHandler.hexoPostsDir + "\\temp\\");
    }

    public async Task Process()
    {
        if (obsidianTempDir.Exists)
            obsidianTempDir.Delete(true);

        Status.CreateStatus("Making backup", () =>
        {
            FileSystemUtils.DeepCopyDirectory(obsidianVaultDir, obsidianTempDir.FullName,
                                              ConfigurationMgr.configuration.ignoresPaths.ToList()).Wait();
        });

        await ConvertObsidianNoteToHexoPostBundles();

        obsidianTempDir.Delete(true);
    }

    private async Task ConvertObsidianNoteToHexoPostBundles()
    {
        var markdownFiles = Directory.GetFiles(obsidianTempDir.FullName, "*.md", SearchOption.AllDirectories)
                                     .Where(file => file.EndsWith(".md"))
                                     .ToList();


        var readyToPublishFiles = markdownFiles.Where(ObsidianNoteUtils.IsReadyToPublish).Select(GetOriginalFilePath)
                                               .ToList();

        await Progress.CreateProgress("Converting Obsidian Notes to Hexo Posts", async progressTask =>
        {
            var postFormatters = (await Task.WhenAll(markdownFiles.Select(CreateHexoPostFormatter)))
                                .Where(f => f != null).ToList();
            
            // Set MaxValue to the actual number of files that will be processed
            progressTask.MaxValue = postFormatters.Count;
            
            await Task.WhenAll(postFormatters.Select(formatter => ProcessSingleFormatter(formatter, progressTask)));
        });

        UpdateReadyFilesToPublished(readyToPublishFiles);

        string GetOriginalFilePath(string tempFilePath)
        {
            string relativePath = Path.GetRelativePath(obsidianTempDir.FullName, tempFilePath);
            return Path.Combine(obsidianVaultDir.FullName, relativePath);
        }

        void UpdateReadyFilesToPublished(List<string> filePaths)
        {
            foreach (string filePath in filePaths)
            {
                try
                {
                    YAMLUtils.UpdateYamlMetadata(filePath, "publishStatus", "published");
                } catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to update status for {filePath}: {ex.Message}");
                }
            }
        }
    }


    /// <summary>
    /// Creates a Hexo post bundle and returns a formatter for the created post.
    /// </summary>
    /// <param name="notePath">Path to the Obsidian note file</param>
    /// <returns>HexoPostFormatter for the created post, or null if the note should not be published</returns>
    private async Task<HexoPostFormatter> CreateHexoPostFormatter(string notePath)
    {
        try
        {
            if (!ObsidianNoteUtils.IsRequiredToBePublished(notePath)) return null;

            string postPath = await HexoUtils.CreateHexoPostBundle(notePath, hexoPostsDir);
            return new HexoPostFormatter(notePath, postPath);
        } catch (Exception e)
        {
            Console.WriteLine($"Error creating post bundle for {notePath}: {e.Message}");
            return null;
        }
    }


    private async Task ProcessSingleFormatter(HexoPostFormatter formatter, Spectre.Console.ProgressTask progressTask)
    {
        try
        {
            await formatter.Format();
        } catch (Exception e)
        {
            Console.WriteLine($"Error processing formatter for {formatter.postPath}: {e.Message}");
            Console.WriteLine($"Stack trace: {e.StackTrace}");
        } finally
        {
            progressTask.Increment(1);
        }
    }
}
