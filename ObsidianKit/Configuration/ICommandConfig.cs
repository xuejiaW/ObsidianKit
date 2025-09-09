namespace ObsidianKit;

public interface ICommandConfig
{
    string CommandName { get; }
    void SetDefaults();
    bool Validate(out List<string> errors);
    void DisplayConfiguration();
}
