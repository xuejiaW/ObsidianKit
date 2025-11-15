using System.Text.Json.Serialization;

namespace ObsidianKit;

public class TemplateConfig : ICommandConfig
{
    [JsonIgnore]
    public string CommandName => "template";

    public Dictionary<string, string> templates { get; set; } = new();

    public void SetDefaults() { templates ??= new Dictionary<string, string>(); }

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        // Validate that all template paths exist
        foreach (var template in templates)
        {
            if (!Directory.Exists(template.Value))
            {
                errors.Add($"Template '{template.Key}' path does not exist: {template.Value}");
            }
        }

        return errors.Count == 0;
    }

    public void DisplayConfiguration()
    {
        Console.WriteLine("        Template Configuration:");
        if (templates.Any())
        {
            Console.WriteLine("          Available Templates:");
            foreach (var template in templates)
            {
                Console.WriteLine($"            {template.Key}: {template.Value}");
            }
        }
        else
        {
            Console.WriteLine("          No templates configured");
        }
    }

    public void AddTemplate(string name, string path)
    {
        templates[name] = path;
    }

    public void RemoveTemplate(string name)
    {
        templates.Remove(name);
    }

    public string GetTemplatePath(string name)
    {
        return templates.GetValueOrDefault(name);
    }
}
