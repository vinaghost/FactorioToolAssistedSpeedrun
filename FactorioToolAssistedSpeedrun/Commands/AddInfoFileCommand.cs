using FactorioToolAssistedSpeedrun.Constants;
using System;
using System.Collections.Generic;
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

            await using var saver = new StreamWriter(filePath, false, Encoding.UTF8);
            saver.WriteLine("{");
            saver.WriteLine($"\t\"name\": \"{Path.GetFileName(FolderLocation)}\",");
            saver.WriteLine($"\t\"version\": \"{TasFileConstants.VERSION}\",");
            saver.WriteLine($"\t\"title\": \"Factorio TAS run\",");
            saver.WriteLine($"\t\"author\": \"Theis+VINAGHOST\",");
            saver.WriteLine($"\t\"factorio_version\": \"2.0\",");
            saver.WriteLine($"\t\"contact\": \"https://github.com/vinaghost/FactorioToolAssistedSpeedrun/issues\",");
            saver.WriteLine($"\t\"description\": \"This run has been made with the help of Factorio Tool Assisted Speedrun\"");
            saver.WriteLine("}");
        }
    }
}