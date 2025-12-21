using System.CommandLine;

namespace ObsidianKit;

internal static class DoctorCommand
{
    internal static Command CreateCommand()
    {
        var doctorCommand = new Command("doctor", "Diagnose and fix Obsidian vault issues");

        doctorCommand.AddCommand(DoctorConflictCommand.CreateCommand());
        doctorCommand.AddCommand(DoctorCleanCommand.CreateCommand());
        doctorCommand.AddCommand(DoctorBloatCommand.CreateCommand());
        doctorCommand.AddCommand(DoctorFormatCommand.CreateCommand());

        return doctorCommand;
    }
}
