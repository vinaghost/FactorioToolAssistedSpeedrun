using AdonisUI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Exceptions;
using FactorioToolAssistedSpeedrun.Models;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public LoadingViewModel LoadingViewModel { get; }

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];

        public MainViewModel()
        {
            LoadingViewModel = new LoadingViewModel();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(LoadingViewModel loadingViewModel)
        {
            LoadingViewModel = loadingViewModel;
        }

        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "No project loaded";

        [RelayCommand]
        private void LoadSettings()
        {
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!string.IsNullOrEmpty(gameDataFile) && File.Exists(gameDataFile))
            {
                Version = Path.GetFileNameWithoutExtension(gameDataFile);
            }

            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!string.IsNullOrEmpty(projectDataFile) && File.Exists(projectDataFile))
            {
                try
                {
                    using var _ = new ProjectDbContext(projectDataFile);
                    ProjectName = Path.GetFileNameWithoutExtension(projectDataFile);
                }
                catch
                {
                    MessageBox.Show("Failed to load project database file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task NewProject()
        {
            LoadingViewModel.Show();
            var dialog = new SaveFileDialog
            {
                Filter = "Tas database (*.db)|*.db",
                FileName = "NewProject.db"
            };
            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;
                if (string.IsNullOrEmpty(filename))
                    return;
                using var context = new ProjectDbContext(filename);
                await context.Database.EnsureCreatedAsync();
                Properties.Settings.Default.ProjectDataFile = filename;
                Properties.Settings.Default.Save();
                ProjectName = Path.GetFileNameWithoutExtension(filename);

                MessageBox.Show("New project database created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            LoadingViewModel.Hide();
        }

        [RelayCommand]
        private async Task DumpFactorioData()
        {
            LoadingViewModel.Show();
            await DumpFactorioDataTask();
            LoadingViewModel.Hide();
        }

        private async Task DumpFactorioDataTask()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Factorio executable (*.exe)|*.exe|Factorio data (*.json)|*.json"
            };
            if (dialog.ShowDialog() != true)
                return;

            var filename = dialog.FileName;
            if (string.IsNullOrEmpty(filename))
                return;

            if (!File.Exists(filename))
                return;

            if (filename.Contains("factorio.exe"))
            {
                var dumpFactorioDataCommand = new DumpFactorioDataCommand
                {
                    FileName = filename
                };
                await dumpFactorioDataCommand.Execute();

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(appData, "Factorio", "script-output", "data-raw-dump.json");
                var fileContent = await File.ReadAllTextAsync(filePath);
                var prototypeData = JsonSerializer.Deserialize<PrototypeData>(fileContent);
                var gameData = new GameData(prototypeData!);

                var gameDataFile = $"{Version}.json";
                await File.WriteAllTextAsync(gameDataFile, JsonSerializer.Serialize(gameData));

                Properties.Settings.Default.GameDataFile = gameDataFile;
                Properties.Settings.Default.Save();
                MessageBox.Show($"Game data dumped and saved to {gameDataFile}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (filename.EndsWith(".json"))
            {
                LoadingViewModel.Show();
                var fileContent = await File.ReadAllTextAsync(filename);
                try
                {
                    var gameData = JsonSerializer.Deserialize<GameData>(fileContent);
                    Version = Path.GetFileNameWithoutExtension(filename);
                    Properties.Settings.Default.GameDataFile = Path.GetFileName(filename);
                    Properties.Settings.Default.Save();

                    MessageBox.Show($"Game data loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Failed to load game data from the selected JSON file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            LoadingViewModel.Show();
            await OpenFileTask();
            LoadingViewModel.Hide();
        }

        private async Task OpenFileTask()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Tas database (*.db)|*.db|Tas files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                var filename = dialog.FileName;
                if (string.IsNullOrEmpty(filename))
                    return;

                if (!File.Exists(filename))
                    return;
                if (filename.EndsWith(".txt"))
                {
                    var parseTasFileCommand = new ParseTasFileCommand
                    {
                        FileName = filename
                    };
                    try
                    {
                        await parseTasFileCommand.Execute();
                    }
                    catch (TasFileParserException ex)
                    {
                        MessageBox.Show($"Failed to parse the TAS file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    var dbFile = Path.Combine(Path.GetDirectoryName(filename)!, $"{Path.GetFileNameWithoutExtension(filename)}.db");

                    var result = MessageBox.Show($"Tool will create a new db file for this project at {dbFile}", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (result == MessageBoxResult.No)
                        return;

                    using var context = new ProjectDbContext(dbFile);
                    await context.Database.EnsureCreatedAsync();

                    using var transaction = await context.Database.BeginTransactionAsync();

                    try
                    {
                        context.Steps.AddRange(parseTasFileCommand.StepCollection);
                        context.Templates.AddRange(parseTasFileCommand.TemplateCollection);
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.MODS_FOLDER_SETTING_KEY,
                            Value = parseTasFileCommand.ModsFolder
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.SelectedRow,
                            Value = parseTasFileCommand.SelectedRow.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.ImportIntoRow,
                            Value = parseTasFileCommand.ImportIntoRow.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintMessage,
                            Value = parseTasFileCommand.PrintMessage.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintSavegame,
                            Value = parseTasFileCommand.PrintSavegame.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintTech,
                            Value = parseTasFileCommand.PrintTech.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.Environment,
                            Value = parseTasFileCommand.Environment.ToString()
                        });

                        await transaction.CommitAsync();

                        await context.SaveChangesAsync();
                        Properties.Settings.Default.ProjectDataFile = dbFile;
                        Properties.Settings.Default.Save();

                        ProjectName = Path.GetFileNameWithoutExtension(dbFile);

                        MessageBox.Show("Database file created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        MessageBox.Show("Failed to create database file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                if (filename.EndsWith(".db"))
                {
                    try
                    {
                        using var _ = new ProjectDbContext(filename);
                        Properties.Settings.Default.ProjectDataFile = filename;
                        Properties.Settings.Default.Save();
                        ProjectName = Path.GetFileNameWithoutExtension(filename);
                        MessageBox.Show("Project database loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load project database file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}