using Spectre.Console;

namespace ObsidianKit.ConsoleUI;

public static class Status
{
    public static void CreateStatus(string message, Action onStatusStart)
    {
        AnsiConsole.Status().Start(message, _ =>
        {
            onStatusStart?.Invoke();
        });
    }
}
