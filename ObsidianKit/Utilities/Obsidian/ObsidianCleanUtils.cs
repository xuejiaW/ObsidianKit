using System.Text.RegularExpressions;

namespace ObsidianKit.Utilities.Obsidian;

public class UnreferencedImage
{
    public string filePath { get; set; }
    public long fileSize { get; set; }
}

public static class ObsidianCleanUtils
{
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
    
    private static readonly Regex MarkdownImagePattern = 
        new Regex(@"!\[.*?\]\(([^)]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex WikiLinkImagePattern = 
        new Regex(@"!\[\[([^\]]+\.(png|jpg|jpeg|gif|webp))\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex YamlImagePattern = 
        new Regex(@"[""']?\[\[([^\]]+\.(png|jpg|jpeg|gif|webp))\]\][""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex HtmlImagePattern = 
        new Regex(@"<img[^>]+src=[""']([^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<UnreferencedImage> FindUnreferencedImages(string vaultPath, HashSet<string> ignorePaths = null)
    {
        var allImages = GetAllImages(vaultPath, ignorePaths);
        var referencedImages = GetReferencedImages(vaultPath, ignorePaths);
        
        var unreferencedImages = allImages
            .Where(img => !IsImageReferenced(img, referencedImages, vaultPath))
            .Select(img => new UnreferencedImage
            {
                filePath = img,
                fileSize = new FileInfo(img).Length
            })
            .OrderByDescending(img => img.fileSize)
            .ToList();

        return unreferencedImages;
    }

    private static List<string> GetAllImages(string vaultPath, HashSet<string> ignorePaths)
    {
        return Directory.EnumerateFiles(vaultPath, "*.*", SearchOption.AllDirectories)
            .Where(file => ImageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
            .Where(file => !IsInIgnoredPath(file, vaultPath, ignorePaths))
            .ToList();
    }

    private static HashSet<string> GetReferencedImages(string vaultPath, HashSet<string> ignorePaths)
    {
        var referencedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var markdownFiles = Directory.EnumerateFiles(vaultPath, "*.md", SearchOption.AllDirectories)
            .Where(file => !IsInIgnoredPath(file, vaultPath, ignorePaths));

        foreach (var mdFile in markdownFiles)
        {
            try
            {
                var content = File.ReadAllText(mdFile);
                var mdFileDir = Path.GetDirectoryName(mdFile);

                var markdownMatches = MarkdownImagePattern.Matches(content);
                foreach (Match match in markdownMatches)
                {
                    var imagePath = match.Groups[1].Value;
                    var resolvedPath = ResolveImagePath(imagePath, mdFileDir, vaultPath);
                    if (resolvedPath != null)
                        referencedImages.Add(resolvedPath);
                }

                var wikiMatches = WikiLinkImagePattern.Matches(content);
                foreach (Match match in wikiMatches)
                {
                    var imagePath = match.Groups[1].Value;
                    var resolvedPath = ResolveWikiLinkPath(imagePath, vaultPath);
                    if (resolvedPath != null)
                        referencedImages.Add(resolvedPath);
                }

                var yamlMatches = YamlImagePattern.Matches(content);
                foreach (Match match in yamlMatches)
                {
                    var imagePath = match.Groups[1].Value;
                    var resolvedPath = ResolveWikiLinkPath(imagePath, vaultPath);
                    if (resolvedPath != null)
                        referencedImages.Add(resolvedPath);
                }

                var htmlMatches = HtmlImagePattern.Matches(content);
                foreach (Match match in htmlMatches)
                {
                    var imagePath = match.Groups[1].Value;
                    var resolvedPath = ResolveImagePath(imagePath, mdFileDir, vaultPath);
                    if (resolvedPath != null)
                        referencedImages.Add(resolvedPath);
                }
            }
            catch
            {
                // Skip files that cannot be read
            }
        }

        return referencedImages;
    }

    private static string ResolveImagePath(string imagePath, string mdFileDir, string vaultPath)
    {
        try
        {
            imagePath = Uri.UnescapeDataString(imagePath);
            
            var fullPath = Path.GetFullPath(Path.Combine(mdFileDir, imagePath));
            
            if (File.Exists(fullPath) && fullPath.StartsWith(vaultPath, StringComparison.OrdinalIgnoreCase))
                return fullPath;
        }
        catch
        {
            // Ignore invalid paths
        }

        return null;
    }

    private static string ResolveWikiLinkPath(string imagePath, string vaultPath)
    {
        try
        {
            // URL decode the path in case it contains encoded characters
            imagePath = Uri.UnescapeDataString(imagePath);
            
            var fileName = Path.GetFileName(imagePath);
            
            var matchingFiles = Directory.EnumerateFiles(vaultPath, fileName, SearchOption.AllDirectories).ToList();
            
            if (matchingFiles.Count == 1)
                return matchingFiles[0];
            
            if (matchingFiles.Count > 1)
            {
                var pathParts = imagePath.Split('/', '\\');
                foreach (var file in matchingFiles)
                {
                    var match = true;
                    for (int i = pathParts.Length - 1, j = 0; i >= 0 && j < 3; i--, j++)
                    {
                        if (!file.Contains(pathParts[i], StringComparison.OrdinalIgnoreCase))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        return file;
                }
                
                return matchingFiles[0];
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    private static bool IsImageReferenced(string imagePath, HashSet<string> referencedImages, string vaultPath)
    {
        if (referencedImages.Contains(imagePath))
            return true;

        var fileName = Path.GetFileName(imagePath);
        foreach (var refImage in referencedImages)
        {
            if (Path.GetFileName(refImage).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsInIgnoredPath(string filePath, string vaultPath, HashSet<string> ignorePaths)
    {
        if (ignorePaths == null || ignorePaths.Count == 0)
            return false;

        var relativePath = Path.GetRelativePath(vaultPath, filePath);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        foreach (var ignorePath in ignorePaths)
        {
            if (pathParts.Any(part => part.Equals(ignorePath, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }
}
