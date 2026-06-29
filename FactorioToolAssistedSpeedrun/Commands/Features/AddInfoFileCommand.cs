using FactorioToolAssistedSpeedrun.Constants;
using System.IO;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands.Features
{
    public static class AddInfoFileCommand
    {
        public static async Task Execute(string folderLocation)
        {
            var filePath = Path.Combine(folderLocation, "info.json");
            if (File.Exists(filePath))
                return;

            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            await writer.WriteLineAsync("{");
            await writer.WriteLineAsync($"\t\"name\": \"{Path.GetFileName(folderLocation)}\",");
            await writer.WriteLineAsync($"\t\"version\": \"{TasFileConstants.VERSION}\",");
            await writer.WriteLineAsync($"\t\"title\": \"Factorio TAS run\",");
            await writer.WriteLineAsync($"\t\"author\": \"Theis+VINAGHOST\",");
            await writer.WriteLineAsync($"\t\"factorio_version\": \"2.1\",");
            await writer.WriteLineAsync($"\t\"contact\": \"https://github.com/vinaghost/FactorioToolAssistedSpeedrun/issues\",");
            await writer.WriteLineAsync($"\t\"description\": \"This run has been made with the help of Factorio Tool Assisted Speedrun\"");
            await writer.WriteLineAsync("}");
        }
    }
}