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
using Microsoft.EntityFrameworkCore;
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
        private GameData? _gameData;

        public MainViewModel()
        {
            LoadingViewModel = new LoadingViewModel();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(LoadingViewModel loadingViewModel)
        {
            LoadingViewModel = loadingViewModel;
        }

        private bool _isLoading = false;

        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "No project loaded";

        private string _projectDataFile = "";
        private string _modsFolder = "";

        [ObservableProperty]
        private bool _printComments = false;

        partial void OnPrintCommentsChanged(bool value)
        {
            if (_isLoading)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;
            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.PrintMessage)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, value ? "1" : "0"));
        }

        [ObservableProperty]
        private bool _printSavegame = true;

        partial void OnPrintSavegameChanged(bool value)
        {
            if (_isLoading)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;
            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.PrintSavegame)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, value ? "1" : "0"));
        }

        [ObservableProperty]
        private bool _printTech = true;

        partial void OnPrintTechChanged(bool value)
        {
            if (_isLoading)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;
            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.PrintTech)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, value ? "1" : "0"));
        }

        [ObservableProperty]
        private bool _debugMode = false;

        partial void OnDebugModeChanged(bool value)
        {
            if (_isLoading)
                return;
            if (!value)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;

            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.Environment)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, "0"));
        }

        [ObservableProperty]
        private bool _developmentMode = true;

        partial void OnDevelopmentModeChanged(bool value)
        {
            if (_isLoading)
                return;
            if (!value)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;

            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.Environment)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, "1"));
        }

        [ObservableProperty]
        private bool _productionMode = false;

        partial void OnProductionModeChanged(bool value)
        {
            if (_isLoading)
                return;
            if (!value)
                return;
            if (string.IsNullOrEmpty(_projectDataFile))
                return;
            if (!File.Exists(_projectDataFile))
                return;

            using var context = new ProjectDbContext(_projectDataFile);
            context.Settings
                .Where(s => s.Key == SettingConstants.Environment)
                .ExecuteUpdate(s => s.SetProperty(s => s.Value, "2"));
        }

        [RelayCommand]
        private void LoadSettings()
        {
            _isLoading = true;
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!string.IsNullOrEmpty(gameDataFile) && File.Exists(gameDataFile))
            {
                Version = Path.GetFileNameWithoutExtension(gameDataFile);
                var fileContent = File.ReadAllText(gameDataFile);
                try
                {
                    _gameData = JsonSerializer.Deserialize<GameData>(fileContent);
                }
                catch
                {
                    MessageBox.Show("Failed to load game data from the saved JSON file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!string.IsNullOrEmpty(projectDataFile) && File.Exists(projectDataFile))
            {
                try
                {
                    using var context = new ProjectDbContext(projectDataFile);
                    _projectDataFile = projectDataFile;

                    var printMessageSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintMessage);
                    if (printMessageSetting is not null)
                    {
                        PrintComments = printMessageSetting.Value == "1";
                    }
                    else
                    {
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintMessage,
                            Value = "0"
                        });
                    }

                    var printSavegameSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintSavegame);
                    if (printSavegameSetting is not null)
                    {
                        PrintSavegame = printSavegameSetting.Value == "1";
                    }
                    else
                    {
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintSavegame,
                            Value = "1"
                        });
                    }

                    var printTechSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.PrintTech);
                    if (printTechSetting is not null)
                    {
                        PrintTech = printTechSetting.Value == "1";
                    }
                    else
                    {
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintTech,
                            Value = "1"
                        });
                    }

                    var environmentSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.Environment);
                    if (environmentSetting is not null)
                    {
                        switch (environmentSetting.Value)
                        {
                            case "0":
                                DebugMode = true;
                                break;

                            case "1":
                                DevelopmentMode = true;
                                break;

                            case "2":
                                ProductionMode = true;
                                break;

                            default:
                                DevelopmentMode = true;
                                break;
                        }
                    }
                    else
                    {
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.Environment,
                            Value = "1"
                        });
                    }

                    var modsFolderSetting = context.Settings.FirstOrDefault(s => s.Key == SettingConstants.MODS_FOLDER_SETTING_KEY);
                    if (modsFolderSetting is not null)
                    {
                        _modsFolder = modsFolderSetting.Value;
                    }
                    else
                    {
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.MODS_FOLDER_SETTING_KEY,
                            Value = ""
                        });
                    }

                    ProjectName = Path.GetFileNameWithoutExtension(projectDataFile);
                }
                catch
                {
                    MessageBox.Show("Failed to load project database file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            _isLoading = false;
        }

        [RelayCommand]
        private async Task ModsFolder()
        {
            if (string.IsNullOrEmpty(_projectDataFile))
            {
                MessageBox.Show("No project loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadingViewModel.Show();

            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                var folderName = dialog.FolderName;
                if (!string.IsNullOrEmpty(folderName))
                {
                    using var context = new ProjectDbContext(_projectDataFile);
                    context.Settings
                        .Where(s => s.Key == SettingConstants.MODS_FOLDER_SETTING_KEY)
                        .ExecuteUpdate(s => s.SetProperty(s => s.Value, folderName));

                    _modsFolder = folderName;

                    MessageBox.Show("Mods folder updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            LoadingViewModel.Hide();
        }

        [RelayCommand]
        private async Task GenerateScript()
        {
            if (string.IsNullOrEmpty(_projectDataFile))
            {
                MessageBox.Show("No project loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_gameData is null)
            {
                MessageBox.Show("No game data loaded. Please dump or load game data first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadingViewModel.Show();
            await GenerateScriptTask();
            LoadingViewModel.Hide();
        }

        private async Task GenerateScriptTask()
        {
            if (string.IsNullOrEmpty(_modsFolder) || !Directory.Exists(_modsFolder))
            {
                var dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() == true)
                {
                    var folderName = dialog.FolderName;
                    if (string.IsNullOrEmpty(folderName))
                    {
                        MessageBox.Show("Script folder path is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    using var context = new ProjectDbContext(_projectDataFile);
                    context.Settings
                        .Where(s => s.Key == SettingConstants.MODS_FOLDER_SETTING_KEY)
                        .ExecuteUpdate(s => s.SetProperty(s => s.Value, folderName));

                    _modsFolder = folderName;
                }
                else
                {
                    MessageBox.Show("Script folder is required to generate the script.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var controlFile = Path.Combine(_modsFolder, "control.lua");
            if (!File.Exists(controlFile))
            {
                File.Copy(Path.Combine("LuaFolders", "control.lua"), controlFile);
            }
            var settingsFile = Path.Combine(_modsFolder, "settings.lua");
            if (!File.Exists(settingsFile))
            {
                File.Copy(Path.Combine("LuaFolders", "settings.lua"), settingsFile);
            }
            var goalFile = Path.Combine(_modsFolder, "goals.lua");
            if (!File.Exists(goalFile))
            {
                File.Copy(Path.Combine("LuaFolders", "goals.lua"), goalFile);
            }
            var localeFile = Path.Combine(_modsFolder, "locale", "en", "locale.cfg");
            if (!File.Exists(localeFile))
            {
                Directory.CreateDirectory(Path.Combine(_modsFolder, "locale", "en"));
                File.Copy(Path.Combine("LuaFolders", "locale", "en", "locale.cfg"), localeFile);
            }

            var addVariableFileCommand = new AddVariableFileCommand
            {
                FolderLocation = _modsFolder,
                EnvironmentId = DebugMode ? 0 : DevelopmentMode ? 1 : 2,
                PrintMessage = PrintComments,
                PrintSavegame = PrintSavegame,
                PrintTech = PrintTech
            };

            await addVariableFileCommand.Execute();

            var addInfoFileCommand = new AddInfoFileCommand
            {
                FolderLocation = _modsFolder,
            };

            await addInfoFileCommand.Execute();

            var addStepFileCommand = new AddStepsFileCommand
            {
                FolderLocation = _modsFolder,
                DbContext = new ProjectDbContext(_projectDataFile),
            };
            await addStepFileCommand.Execute();
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
                context.SetupTriggers();
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

                Version = dumpFactorioDataCommand.Result;

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(appData, "Factorio", "script-output", "data-raw-dump.json");
                var fileContent = await File.ReadAllTextAsync(filePath);
                var prototypeData = JsonSerializer.Deserialize<PrototypeData>(fileContent);
                var gameData = new GameData(prototypeData!);

                _gameData = gameData;

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
                    _gameData = gameData;

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
            if (_gameData is null)
            {
                MessageBox.Show("No game data loaded. Please dump or load game data first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
                        FileName = filename,
                        GameData = _gameData!
                    };
                    try
                    {
                        await parseTasFileCommand.Execute();
                    }
                    catch (TasFileParserException ex)
                    {
                        MessageBox.Show($"Failed to parse the TAS file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    var tasFileResult = parseTasFileCommand.Result;

                    var dbFile = Path.Combine(Path.GetDirectoryName(filename)!, $"{Path.GetFileNameWithoutExtension(filename)}.db");

                    var result = MessageBox.Show($"Tool will create a new db file for this project at {dbFile} (existing file will be overrided) ", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (result == MessageBoxResult.No)
                        return;

                    using var context = new ProjectDbContext(dbFile);

                    try
                    {
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.EnsureCreatedAsync();
                        context.SetupTriggers();

                        context.Steps.AddRange(tasFileResult.StepCollection);
                        context.Templates.AddRange(tasFileResult.TemplateCollection);
                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.MODS_FOLDER_SETTING_KEY,
                            Value = tasFileResult.ModsFolder
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.SelectedRow,
                            Value = tasFileResult.SelectedRow.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.ImportIntoRow,
                            Value = tasFileResult.ImportIntoRow.ToString()
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintMessage,
                            Value = tasFileResult.PrintMessage ? "1" : "0"
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintSavegame,
                            Value = tasFileResult.PrintSavegame ? "1" : "0"
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.PrintTech,
                            Value = tasFileResult.PrintTech ? "1" : "0"
                        });

                        context.Settings.Add(new Setting
                        {
                            Key = SettingConstants.Environment,
                            Value = tasFileResult.Environment.ToString()
                        });

                        await context.SaveChangesAsync();
                        Properties.Settings.Default.ProjectDataFile = dbFile;
                        Properties.Settings.Default.Save();

                        ProjectName = Path.GetFileNameWithoutExtension(dbFile);

                        MessageBox.Show("Database file created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is not null)
                        {
                            ex = ex.InnerException;
                        }
                        MessageBox.Show($"Failed to create database file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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