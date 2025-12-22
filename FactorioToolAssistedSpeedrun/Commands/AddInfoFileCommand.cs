using FactorioToolAssistedSpeedrun.Constants;
using System.IO;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands
{
    public class AddInfoFileCommand : ICommand
    {
        public required string FolderLocation { get; init; }

        public async Task Execute()
        {
            var filePath = Path.Combine(FolderLocation, "info.json");
            if (File.Exists(filePath))
            {
                return;
            }

            await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.WriteLine("{");
            writer.WriteLine($"\t\"name\": \"{Path.GetFileName(FolderLocation)}\",");
            writer.WriteLine($"\t\"version\": \"{TasFileConstants.VERSION}\",");
            writer.WriteLine($"\t\"title\": \"Factorio TAS run\",");
            writer.WriteLine($"\t\"author\": \"Theis+VINAGHOST\",");
            writer.WriteLine($"\t\"factorio_version\": \"2.0\",");
            writer.WriteLine($"\t\"contact\": \"https://github.com/vinaghost/FactorioToolAssistedSpeedrun/issues\",");
            writer.WriteLine($"\t\"description\": \"This run has been made with the help of Factorio Tool Assisted Speedrun\"");
            writer.WriteLine("}");
        }
    }
}