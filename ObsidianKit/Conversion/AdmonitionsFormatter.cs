using System.Text;
using System.Text.RegularExpressions;
using ObsidianKit.Utilities.Admonition;

namespace ObsidianKit;

public static class AdmonitionsFormatter
{
    public static string FormatCodeBlockStyle2ButterflyStyle(string content)
    {
        var mapping = AdmonitionMappingUtils.GetCodeBlockToButterflyMapping();
        return AdmonitionStyleUtils.ConvertCodeBlockToButterflyStyle(content, mapping);
    }

    public static string FormatMkDocsStyle2ButterflyStyle(string content)
    {
        var mapping = AdmonitionMappingUtils.GetMkDocsToButterflyMapping();
        return AdmonitionStyleUtils.ConvertMkDocsToButterflyStyle(content, mapping);
    }

    public static string FormatMkDocsCalloutToQuote(string content)
    {
        return AdmonitionStyleUtils.ConvertMkDocsCalloutToQuote(content);
    }
}
