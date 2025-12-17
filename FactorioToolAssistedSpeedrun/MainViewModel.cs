using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FactorioToolAssistedSpeedrun
{
    public partial class MainViewModel
    {
        public MainViewModel()
        {
            BooksCollection = new ObservableCollection<StepModel>
            {
                new StepModel { Step = "Walk", X = 0, Y = 0, Amount = 0, Item = "", Orientation = "N/A", Modifier = "", Comment = "Start position" },
                new StepModel { Step = "Craft", X = 5.5, Y = 7.2, Amount = 1, Item = "Iron Gear", Orientation = "N/A", Modifier = "", Comment = "First craft" },
                new StepModel { Step = "Walk", X = 10, Y = 15, Amount = 0, Item = "", Orientation = "East", Modifier = "Fast", Comment = "Move to next area" },
                new StepModel { Step = "Tech", X = 0, Y = 0, Amount = 0, Item = "Automation", Orientation = "N/A", Modifier = "", Comment = "Research automation" },
                new StepModel { Step = "Craft", X = 12.3, Y = 8.8, Amount = 2, Item = "Transport Belt", Orientation = "N/A", Modifier = "", Comment = "Prepare belts" }
            };
        }

        public ObservableCollection<StepModel> BooksCollection { get; set; }

        [RelayCommand]
        private async Task OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;
                if (string.IsNullOrEmpty(filename))
                    return;

                if (!File.Exists(filename))
                    return;
                if (filename.EndsWith(".txt"))
                {
                    using var file = new FileStream(filename, FileMode.Open);
                    return;
                }

                if (filename.Contains("data-raw-dump.json"))
                {
                    var fileContent = await File.ReadAllTextAsync(filename);
                    var prototypeData = JsonSerializer.Deserialize<PrototypeData>(fileContent);
                    _ = prototypeData;
                }

                if (filename.Contains("factorio.exe"))
                {
                    await Task.Run(() =>
                    {
                        using var process = new Process()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = filename,
                                Arguments = "--dump-data",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                    });
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string filePath = Path.Combine(appData, "Factorio", "script-output", "data-raw-dump.json");
                    _ = File.Exists(filePath);
                }
            }
        }
    }
}