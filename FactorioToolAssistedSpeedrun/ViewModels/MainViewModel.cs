using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public LoadingViewModel LoadingViewModel { get; }
        public StepTypeViewModel StepTypeViewModel { get; }

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];
        private GameData? _gameData;

        public MainViewModel()
        {
            LoadingViewModel = new LoadingViewModel();
            StepTypeViewModel = new StepTypeViewModel();
        }

        [ActivatorUtilitiesConstructor]
        public MainViewModel(LoadingViewModel loadingViewModel, StepTypeViewModel stepTypeViewModel)
        {
            LoadingViewModel = loadingViewModel;
            StepTypeViewModel = stepTypeViewModel;
        }

        private bool _isLoading = false;

        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "No project loaded";

        [ObservableProperty]
        private string _scriptFolder = "";

        private string _projectDataFile = "";

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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.PrintMessage,
                Value = value ? "1" : "0"
            };

            updateSettingCommand.Execute();
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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.PrintSavegame,
                Value = value ? "1" : "0"
            };
            updateSettingCommand.Execute();
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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.PrintTech,
                Value = value ? "1" : "0"
            };
            updateSettingCommand.Execute();
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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.Environment,
                Value = "0"
            };
            updateSettingCommand.Execute();
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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.Environment,
                Value = "1"
            };
            updateSettingCommand.Execute();
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

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.Environment,
                Value = "2"
            };

            updateSettingCommand.Execute();
        }

        [RelayCommand]
        private async Task LoadSettings()
        {
            _isLoading = true;

            try
            {
                LoadGameDataFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                await LoadProjectDataFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project data file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _isLoading = false;
        }

        private void LoadGameDataFile()
        {
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!File.Exists(gameDataFile))
                return;

            Version = Path.GetFileNameWithoutExtension(gameDataFile);

            var fileContent = File.ReadAllText(gameDataFile);
            _gameData = JsonSerializer.Deserialize<GameData>(fileContent);
        }

        private async Task LoadProjectDataFile()
        {
            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!File.Exists(projectDataFile))
                return;

            var loadSettingsCommand = new LoadSettingsCommand
            {
                ProjectDataFile = projectDataFile
            };

            await Task.Run(loadSettingsCommand.Execute);

            var settingsResult = loadSettingsCommand.Result;

            PrintComments = settingsResult.PrintComments;
            PrintSavegame = settingsResult.PrintSavegame;
            PrintTech = settingsResult.PrintTech;

            DebugMode = settingsResult.DebugMode;
            DevelopmentMode = settingsResult.DevelopmentMode;
            ProductionMode = settingsResult.ProductionMode;
            ScriptFolder = settingsResult.ScriptFolder;

            _projectDataFile = projectDataFile;
            ProjectName = Path.GetFileNameWithoutExtension(projectDataFile);
        }

        [RelayCommand]
        private async Task SetScriptLocation()
        {
            if (string.IsNullOrEmpty(_projectDataFile))
            {
                MessageBox.Show("No project loaded. Please open project first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() != true)
                return;

            var folderName = dialog.FolderName;
            if (string.IsNullOrEmpty(folderName))
                return;

            LoadingViewModel.Show();

            try
            {
                await SetScriptLocationTask(folderName);
                MessageBox.Show("Script location set successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set script location. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadingViewModel.Hide();
        }

        private async Task SetScriptLocationTask(string folderName)
        {
            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = _projectDataFile,
                Setting = SettingConstants.ScriptFolder,
                Value = folderName
            };
            await Task.Run(updateSettingCommand.Execute);
            ScriptFolder = folderName;
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

            if (string.IsNullOrEmpty(ScriptFolder) || !Directory.Exists(ScriptFolder))
            {
                var dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() == true)
                {
                    var folderName = dialog.FolderName;
                    if (string.IsNullOrEmpty(folderName))
                    {
                        MessageBox.Show("Script location path is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var updateSettingCommand = new UpdateSettingCommand
                    {
                        ProjectDataFile = _projectDataFile,
                        Setting = SettingConstants.ScriptFolder,
                        Value = folderName
                    };

                    ScriptFolder = folderName;
                }
                else
                {
                    MessageBox.Show("Script location is required to generate the script.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadingViewModel.Show();
            try
            {
                await GenerateScriptTask();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate script. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadingViewModel.Hide();
        }

        private async Task GenerateScriptTask()
        {
            var controlFile = Path.Combine(ScriptFolder, "control.lua");
            if (!File.Exists(controlFile))
            {
                File.Copy(Path.Combine("LuaFolders", "control.lua"), controlFile);
            }
            var settingsFile = Path.Combine(ScriptFolder, "settings.lua");
            if (!File.Exists(settingsFile))
            {
                File.Copy(Path.Combine("LuaFolders", "settings.lua"), settingsFile);
            }
            var goalFile = Path.Combine(ScriptFolder, "goals.lua");
            if (!File.Exists(goalFile))
            {
                File.Copy(Path.Combine("LuaFolders", "goals.lua"), goalFile);
            }
            var localeFile = Path.Combine(ScriptFolder, "locale", "en", "locale.cfg");
            if (!File.Exists(localeFile))
            {
                Directory.CreateDirectory(Path.Combine(ScriptFolder, "locale", "en"));
                File.Copy(Path.Combine("LuaFolders", "locale", "en", "locale.cfg"), localeFile);
            }

            var addVariableFileCommand = new AddVariableFileCommand
            {
                FolderLocation = ScriptFolder,
                EnvironmentId = DebugMode ? 0 : DevelopmentMode ? 1 : 2,
                PrintMessage = PrintComments,
                PrintSavegame = PrintSavegame,
                PrintTech = PrintTech
            };

            await addVariableFileCommand.Execute();

            var addInfoFileCommand = new AddInfoFileCommand
            {
                FolderLocation = ScriptFolder,
            };

            await addInfoFileCommand.Execute();

            var addStepFileCommand = new AddStepsFileCommand
            {
                FolderLocation = ScriptFolder,
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
            var dialog = new OpenFileDialog
            {
                Filter = "Factorio executable (*.exe)|*.exe|Factorio data (*.json)|*.json"
            };
            if (dialog.ShowDialog() != true)
                return;

            var filename = dialog.FileName;
            if (!File.Exists(filename))
                return;

            LoadingViewModel.Show();
            if (filename.EndsWith("factorio.exe"))
            {
                try
                {
                    await DumpFactorioDataTask(filename);
                    MessageBox.Show($"Game data dumped successfully. Version: {Version}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to dump game data from the selected Factorio executable. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (filename.EndsWith(".json"))
            {
                try
                {
                    await LoadFactorioDataTask(filename);
                    MessageBox.Show($"Game data loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load game data from the selected JSON file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            LoadingViewModel.Hide();
        }

        private async Task DumpFactorioDataTask(string filename)
        {
            var dumpFactorioDataCommand = new DumpFactorioDataCommand
            {
                FileName = filename
            };
            await Task.Run(dumpFactorioDataCommand.Execute);
            Version = dumpFactorioDataCommand.Result;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(appData, "Factorio", "script-output", "data-raw-dump.json");
            var fileContent = File.OpenRead(filePath);
            var prototypeData = await JsonSerializer.DeserializeAsync<PrototypeData>(fileContent);
            var gameData = new GameData(prototypeData!);

            _gameData = gameData;

            var gameDataFile = $"{Version}.json";
            await File.WriteAllTextAsync(gameDataFile, JsonSerializer.Serialize(gameData));

            Properties.Settings.Default.GameDataFile = gameDataFile;
            Properties.Settings.Default.Save();
        }

        private async Task LoadFactorioDataTask(string filename)
        {
            var fileContent = File.OpenRead(filename);

            var gameData = await JsonSerializer.DeserializeAsync<GameData>(fileContent);

            _gameData = gameData;
            Version = Path.GetFileNameWithoutExtension(filename);

            Properties.Settings.Default.GameDataFile = Path.GetFileName(filename);
            Properties.Settings.Default.Save();
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if (_gameData is null)
            {
                MessageBox.Show("No game data loaded. Please dump or load game data first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Tas database (*.db)|*.db|Tas files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() != true)
                return;

            var filename = dialog.FileName;
            if (!File.Exists(filename))
                return;

            LoadingViewModel.Show();
            if (filename.EndsWith(".txt"))
            {
                try
                {
                    await MigrateTasFile(filename);
                    MessageBox.Show("Tas file migrated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is not null) ex = ex.InnerException;
                    MessageBox.Show($"Failed to migrate TAS file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (filename.EndsWith(".db"))
            {
                try
                {
                    await OpenFileTask(filename);
                    MessageBox.Show("Project database file opened successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is not null) ex = ex.InnerException;
                    MessageBox.Show($"Failed to open project database file. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            LoadingViewModel.Hide();
        }

        private async Task MigrateTasFile(string filename)
        {
            var parseTasFileCommand = new ParseTasFileCommand
            {
                FileName = filename,
                GameData = _gameData!
            };

            await Task.Run(parseTasFileCommand.Execute);
            var tasFileResult = parseTasFileCommand.Result;

            var dbFile = Path.Combine(Path.GetDirectoryName(filename)!, $"{Path.GetFileNameWithoutExtension(filename)}.db");

            var result = MessageBox.Show($"Tool will create a new db file for this project at {dbFile} (existing file will be overrided) ", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No)
                return;

            var migrateTasFileDataCommand = new MigrateTasFileDataCommand
            {
                ProjectDataFile = dbFile,
                TasFileResult = tasFileResult
            };
            await Task.Run(migrateTasFileDataCommand.Execute);

            PrintComments = tasFileResult.PrintComments;
            PrintSavegame = tasFileResult.PrintSavegame;
            PrintTech = tasFileResult.PrintTech;

            DebugMode = tasFileResult.Environment == 0;
            DevelopmentMode = tasFileResult.Environment == 1;
            ProductionMode = tasFileResult.Environment == 2;
            ScriptFolder = tasFileResult.ScriptFolder;

            Properties.Settings.Default.ProjectDataFile = dbFile;
            Properties.Settings.Default.Save();

            ProjectName = Path.GetFileNameWithoutExtension(dbFile);
        }

        private async Task OpenFileTask(string filename)
        {
            using var _ = new ProjectDbContext(filename);
            Properties.Settings.Default.ProjectDataFile = filename;
            Properties.Settings.Default.Save();
            ProjectName = Path.GetFileNameWithoutExtension(filename);
        }
    }
}