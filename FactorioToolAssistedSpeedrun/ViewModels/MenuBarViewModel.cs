using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MenuBarViewModel : ObservableObject
    {
        public LoadingViewModel LoadingViewModel { get; }

        public MenuBarViewModel(LoadingViewModel loadingViewModel)
        {
            LoadingViewModel = loadingViewModel;
        }

        public MenuBarViewModel()
        {
            LoadingViewModel = new LoadingViewModel();
        }

        public void UpdateSetting(SettingsResult settingsResult)
        {
            PrintComments = settingsResult.PrintComments;
            PrintSavegame = settingsResult.PrintSavegame;
            PrintTech = settingsResult.PrintTech;

            DebugMode = settingsResult.DebugMode;
            DevelopmentMode = settingsResult.DevelopmentMode;
            ProductionMode = settingsResult.ProductionMode;
            ScriptFolder = settingsResult.ScriptFolder;
        }

        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "No project loaded";

        [ObservableProperty]
        private string _scriptFolder = "";

        [RelayCommand]
        private async Task SetScriptLocation()
        {
            if (string.IsNullOrEmpty(App.Current.ProjectDataFile))
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
                await SetScriptLocationTask(App.Current.ProjectDataFile, folderName);
                MessageBox.Show("Script location set successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set script location. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadingViewModel.Hide();
        }

        private async Task SetScriptLocationTask(string projectDataFile, string folderName)
        {
            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = projectDataFile,
                Setting = SettingConstants.ScriptFolder,
                Value = folderName
            };
            await Task.Run(updateSettingCommand.Execute);
            ScriptFolder = folderName;
        }

        [RelayCommand]
        private async Task GenerateScript()
        {
            if (string.IsNullOrEmpty(App.Current.ProjectDataFile))
            {
                MessageBox.Show("No project loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (App.Current.GameData is null)
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
                        ProjectDataFile = App.Current.ProjectDataFile,
                        Setting = SettingConstants.ScriptFolder,
                        Value = folderName
                    };

                    await Task.Run(updateSettingCommand.Execute);

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
                await GenerateScriptTask(App.Current.ProjectDataFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate script. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadingViewModel.Hide();
        }

        private async Task GenerateScriptTask(string projectDataFile)
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
                DbContext = new ProjectDbContext(projectDataFile),
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

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var scriptOutputDir = Path.Combine(appData, "Factorio", "script-output");
            var dataRawDumpFile = Path.Combine(scriptOutputDir, "data-raw-dump.json");
            using var dataRawDumpFileContent = File.OpenRead(dataRawDumpFile);
            var prototypeData = await JsonSerializer.DeserializeAsync<PrototypeData>(dataRawDumpFileContent);

            var itemLocaleFile = Path.Combine(scriptOutputDir, "item-locale.json");
            using var itemLocaleFileContent = File.OpenRead(itemLocaleFile);
            var itemLocaleData = await JsonSerializer.DeserializeAsync<LocalePrototype>(itemLocaleFileContent);

            var recipeLocaleFile = Path.Combine(scriptOutputDir, "recipe-locale.json");
            using var recipeLocaleFileContent = File.OpenRead(recipeLocaleFile);
            var recipeLocaleData = await JsonSerializer.DeserializeAsync<LocalePrototype>(recipeLocaleFileContent);

            var technologyLocaleFile = Path.Combine(scriptOutputDir, "technology-locale.json");
            using var technologyLocaleFileContent = File.OpenRead(technologyLocaleFile);
            var technologyLocaleData = await JsonSerializer.DeserializeAsync<LocalePrototype>(technologyLocaleFileContent);

            var gameData = GameData.Create(prototypeData!, technologyLocaleData!, itemLocaleData!, recipeLocaleData!);

            App.Current.GameData = gameData;

            var gameDataFile = $"{Version}.json";
            await File.WriteAllTextAsync(gameDataFile, JsonSerializer.Serialize(gameData));

            Properties.Settings.Default.GameDataFile = gameDataFile;
            Properties.Settings.Default.Save();
        }

        private async Task LoadFactorioDataTask(string filename)
        {
            using var fileContent = File.OpenRead(filename);

            var gameData = await JsonSerializer.DeserializeAsync<GameData>(fileContent);

            App.Current.GameData = gameData;
            Version = Path.GetFileNameWithoutExtension(filename);

            Properties.Settings.Default.GameDataFile = Path.GetFileName(filename);
            Properties.Settings.Default.Save();
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if (App.Current.GameData is null)
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
                GameData = App.Current.GameData!
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
            using var context = new ProjectDbContext(filename);
            if (context.Settings.Any())
            {
                Properties.Settings.Default.ProjectDataFile = filename;
                Properties.Settings.Default.Save();
                ProjectName = Path.GetFileNameWithoutExtension(filename);
            }
            else
            {
                throw new Exception("The selected database file is not a valid project database.");
            }
        }

        [ObservableProperty]
        private bool _printComments = false;

        partial void OnPrintCommentsChanged(bool value)
        {
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.PrintMessage,
                Value = value ? "1" : "0"
            };

            updateSettingCommand.Execute();
        }

        [ObservableProperty]
        private bool _printSavegame = true;

        partial void OnPrintSavegameChanged(bool value)
        {
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.PrintSavegame,
                Value = value ? "1" : "0"
            };
            updateSettingCommand.Execute();
        }

        [ObservableProperty]
        private bool _printTech = true;

        partial void OnPrintTechChanged(bool value)
        {
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.PrintTech,
                Value = value ? "1" : "0"
            };
            updateSettingCommand.Execute();
        }

        [ObservableProperty]
        private bool _debugMode = false;

        partial void OnDebugModeChanged(bool value)
        {
            if (!value)
                return;
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.Environment,
                Value = "0"
            };
            updateSettingCommand.Execute();
        }

        [ObservableProperty]
        private bool _developmentMode = true;

        partial void OnDevelopmentModeChanged(bool value)
        {
            if (!value)
                return;
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.Environment,
                Value = "1"
            };
            updateSettingCommand.Execute();
        }

        [ObservableProperty]
        private bool _productionMode = false;

        partial void OnProductionModeChanged(bool value)
        {
            if (!value)
                return;
            if (!File.Exists(App.Current.ProjectDataFile))
                return;

            var updateSettingCommand = new UpdateSettingCommand
            {
                ProjectDataFile = App.Current.ProjectDataFile,
                Setting = SettingConstants.Environment,
                Value = "2"
            };

            updateSettingCommand.Execute();
        }
    }
}