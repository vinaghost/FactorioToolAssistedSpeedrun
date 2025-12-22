using FactorioToolAssistedSpeedrun.Constants;
using System.IO;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class AddVariableFileCommand : ICommand
    {
        public required string FolderLocation { get; init; }
        public required int EnvironmentId { get; init; }
        public required bool PrintMessage { get; init; }
        public required bool PrintSavegame { get; init; }
        public required bool PrintTech { get; init; }

        public async Task Execute()
        {
            var filePath = Path.Combine(FolderLocation, "variables.lua");
            if (File.Exists(filePath))
            {
                return;
            }

            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("--[[ GENERATED FILE - do not modify this file as it is controlled from the FTG GUI ]]");
            writer.WriteLine();

            writer.WriteLine($"GOAL = \"Any%\"");
            writer.WriteLine($"LOGLEVEL = {EnvironmentId}");
            writer.WriteLine($"PRINT_SAVEGAME = {BoolToString(PrintSavegame)}");
            writer.WriteLine($"PRINT_TECH = {BoolToString(PrintTech)}");
            writer.WriteLine($"PRINT_COMMENT = {BoolToString(PrintMessage)}");
            writer.WriteLine();

            writer.WriteLine("local tas_generator = {");
            writer.WriteLine($"\tname = \"Factorio Tool Assisted Speedrun\",");
            writer.WriteLine($"\tversion = \"{TasFileConstants.VERSION}\",");
            writer.WriteLine("\ttas = {");
            writer.WriteLine($"\t\tname = \"{Path.GetFileName(FolderLocation)}\",");
            writer.WriteLine($"\t\ttimestamp = \"{CurrentDateTime()}\",");
            writer.WriteLine("\t},");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("return tas_generator");
        }

        private static string BoolToString(bool value) => value ? "true" : "false";

        private static string CurrentDateTime() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}