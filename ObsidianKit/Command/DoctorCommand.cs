using System.CommandLine;

namespace ObsidianKit;

internal static class DoctorCommand
{
    internal static Command CreateCommand()
    {
        var doctorCommand = new Command("doctor", "Diagnose and fix Obsidian vault issues");

        doctorCommand.AddCommand(DoctorConflictCommand.CreateCommand());

        return doctorCommand;
    }
}
