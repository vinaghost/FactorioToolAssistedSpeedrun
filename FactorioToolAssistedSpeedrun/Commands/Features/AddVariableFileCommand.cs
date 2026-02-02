using FactorioToolAssistedSpeedrun.Constants;
using System.IO;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands.Features
{
    public record VariableObject(int EnvironmentId, bool PrintMessage, bool PrintSavegame, bool PrintTech);

    public static class AddVariableFileCommand
    {
        public static async Task Execute(string folderLocation, VariableObject variables)
        {
            var filePath = Path.Combine(folderLocation, "variables.lua");
            if (File.Exists(filePath))
            {
                return;
            }

            var (environmentId, printMessage, printSavegame, printTech) = variables;

            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            await writer.WriteLineAsync("--[[ GENERATED FILE - do not modify this file as it is controlled from the FTG GUI ]]");
            await writer.WriteLineAsync();

            await writer.WriteLineAsync($"GOAL = \"Any%\"");
            await writer.WriteLineAsync($"LOGLEVEL = {environmentId}");
            await writer.WriteLineAsync($"PRINT_SAVEGAME = {BoolToString(printSavegame)}");
            await writer.WriteLineAsync($"PRINT_TECH = {BoolToString(printTech)}");
            await writer.WriteLineAsync($"PRINT_COMMENT = {BoolToString(printMessage)}");
            await writer.WriteLineAsync();

            await writer.WriteLineAsync("local tas_generator = {");
            await writer.WriteLineAsync($"\tname = \"Factorio Tool Assisted Speedrun\",");
            await writer.WriteLineAsync($"\tversion = \"{TasFileConstants.VERSION}\",");
            await writer.WriteLineAsync("\ttas = {");
            await writer.WriteLineAsync($"\t\tname = \"{Path.GetFileName(folderLocation)}\",");
            await writer.WriteLineAsync($"\t\ttimestamp = \"{CurrentDateTime()}\",");
            await writer.WriteLineAsync("\t},");
            await writer.WriteLineAsync("}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("return tas_generator");
        }

        private static string BoolToString(bool value) => value ? "true" : "false";

        private static string CurrentDateTime() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}