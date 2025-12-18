using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            StepCollection = new ObservableCollection<StepModel>
            {
                new StepModel { Step = "Walk", X = 0, Y = 0, Amount = 0, Item = "", Orientation = "N/A", Modifier = "", Comment = "Start position" },
                new StepModel { Step = "Craft", X = 5.5, Y = 7.2, Amount = 1, Item = "Iron Gear", Orientation = "N/A", Modifier = "", Comment = "First craft" },
                new StepModel { Step = "Walk", X = 10, Y = 15, Amount = 0, Item = "", Orientation = "East", Modifier = "Fast", Comment = "Move to next area" },
                new StepModel { Step = "Tech", X = 0, Y = 0, Amount = 0, Item = "Automation", Orientation = "N/A", Modifier = "", Comment = "Research automation" },
                new StepModel { Step = "Craft", X = 12.3, Y = 8.8, Amount = 2, Item = "Transport Belt", Orientation = "N/A", Modifier = "", Comment = "Prepare belts" }
            };
        }

        public ObservableCollection<StepModel> StepCollection { get; set; }

        [ObservableProperty]
        private string _version = "Not loaded";

        [RelayCommand]
        private async Task Factorio()

        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Factorio executable (*.exe)|*.exe";
            if (dialog.ShowDialog() != true) return;

            var filename = dialog.FileName;
            if (string.IsNullOrEmpty(filename))
                return;

            if (!File.Exists(filename))
                return;

            if (!filename.Contains("factorio.exe"))
                return;

            Version = await Task.Run(() =>
            {
                using var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filename,
                        Arguments = "--dump-data",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    }
                };
                var version = "Not loaded";
                var outputBuilder = new StringBuilder();
                void OutputDataReceivedHandler(object sender, DataReceivedEventArgs args)
                {
                    if (string.IsNullOrEmpty(args.Data)) return;

                    var match = Regex.Match(args.Data, @"\d+\.\d+\.\d+");
                    if (match.Success)
                    {
                        version = match.Value;
                    }

                    process.OutputDataReceived -= OutputDataReceivedHandler;
                }
                process.OutputDataReceived += OutputDataReceivedHandler;

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                return version;
            });

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(appData, "Factorio", "script-output", "data-raw-dump.json");
            _ = File.Exists(filePath);
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;
                if (string.IsNullOrEmpty(filename))
                    return;

                if (!File.Exists(filename))
                    return;
                if (filename.EndsWith(".txt"))
                {
                    using var sr = File.OpenText(filename);

                    var line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Equals(TasFileConstants.TOTAL_STEPS_INDICATOR))
                    {
                        var totalStepsLine = sr.ReadLine();
                        if (int.TryParse(totalStepsLine, out int totalSteps))
                        {
                            _ = totalSteps;
                        }
                    }

                    line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Equals(TasFileConstants.GOAL_INDICATOR))
                    {
                        var goalLine = sr.ReadLine();
                        _ = goalLine;
                    }

                    line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Equals(TasFileConstants.STEPS_INDICATOR))
                    {
                        StepCollection.Clear();
                        var stepLine = sr.ReadLine();

                        var steps = new List<StepModel>();
                        while (stepLine is not null)
                        {
                            if (stepLine.Equals(TasFileConstants.TEMPLATES_INDICATOR))
                            {
                                break;
                            }
                            var segments = stepLine.Split(';');
                            var step = new StepModel()
                            {
                                Step = segments.Length > 0 ? segments[0] : "Unknow",
                                X = segments.Length > 1 && double.TryParse(segments[1], out double x) ? x : 0,
                                Y = segments.Length > 2 && double.TryParse(segments[2], out double y) ? y : 0,
                                Amount = segments.Length > 3 && int.TryParse(segments[3], out int amount) ? amount : 0,
                                Item = segments.Length > 4 ? segments[4] : "",
                                Orientation = segments.Length > 5 ? segments[5] : "",
                                Comment = segments.Length > 6 ? segments[6] : "",
                                Color = segments.Length > 7 ? segments[7] : "",
                                Modifier = segments.Length > 8 ? segments[8] : "",
                            };

                            steps.Add(step);
                            stepLine = sr.ReadLine();
                        }
                        for (var i = 0; i < 10; i++)
                        {
                            foreach (var step in steps)
                            {
                                StepCollection.Add(step);
                            }
                        }

                        line = stepLine;
                    }

                    if (line is null) return;

                    if (line.Equals(TasFileConstants.TEMPLATES_INDICATOR))
                    {
                        var templates = new List<string>();
                        while (line is not null)
                        {
                            if (line.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
                            {
                                break;
                            }
                            templates.Add(line);
                            line = sr.ReadLine();
                        }

                        _ = templates;
                    }
                    if (line is null) return;
                    if (line.Equals(TasFileConstants.SAVE_FILE_INDICATOR))
                    {
                        var saveFileLine = sr.ReadLine();
                        _ = saveFileLine;
                    }

                    line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Equals(TasFileConstants.CODE_FILE_INDICATOR))
                    {
                        var codeFileLine = sr.ReadLine();
                        _ = codeFileLine;
                    }

                    line = sr.ReadLine();
                    if (line is null) return;

                    if (line.Contains(TasFileConstants.SELECTED_ROW_INDICATOR))
                    {
                        var selectedRowLine = line.Split(";");
                        _ = selectedRowLine;
                    }

                    line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Contains(TasFileConstants.IMPORT_INTO_ROW_INDICATOR))
                    {
                        var importIntoLine = line.Split(";");
                        _ = importIntoLine;
                    }

                    line = sr.ReadLine();
                    if (line is null) return;
                    if (line.Contains(TasFileConstants.LOGGING_INDICATOR))
                    {
                        var loggingLine = line.Split(";");
                        _ = loggingLine;
                    }
                }
                if (filename.Contains("data-raw-dump.json"))
                {
                    var fileContent = await File.ReadAllTextAsync(filename);
                    var prototypeData = JsonSerializer.Deserialize<PrototypeData>(fileContent);
                    _ = prototypeData;
                }
            }
        }
    }
}