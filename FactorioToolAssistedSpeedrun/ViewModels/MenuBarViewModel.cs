using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Commands;
using FactorioToolAssistedSpeedrun.Commands.Features;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.Prototypes;
using FactorioToolAssistedSpeedrun.Queries;
using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class MenuBarViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly LoadingService _loadingService;

        private readonly ICommandStack _commandStack;
        public LoadingService LoadingService => _loadingService;

        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "Not loaded";

        public MenuBarViewModel()
        {
            _dataService = Ioc.Default.GetRequiredService<IDataService>();
            _loadingService = Ioc.Default.GetRequiredService<LoadingService>();
            _commandStack = Ioc.Default.GetRequiredService<ICommandStack>();
        }

        [ActivatorUtilitiesConstructor]
        public MenuBarViewModel(IDataService dataService, LoadingService loadingService, ICommandStack commandStack)
        {
            _dataService = dataService;
            _loadingService = loadingService;
            _commandStack = commandStack;

            _dataService.OnProjectDataLoaded += OnProjectDataLoaded;
            _dataService.OnGameDataLoaded += OnGameDataLoaded;
        }

        private void OnGameDataLoaded()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Version = _dataService.Version;
            });
        }

        private void OnProjectDataLoaded()
        {
            var getSettingsQuery = new GetSettingsQuery
            {
                ProjectDataFile = _dataService.ProjectDataFile
            };
            var result = getSettingsQuery.Execute();
            App.Current.Dispatcher.Invoke(() =>
            {
                UpdateSetting(result);
                ProjectName = _dataService.ProjectName;
            });
        }

        private void UpdateSetting(SettingsResult settingsResult)
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
        private string _scriptFolder = "";

        [RelayCommand]
        private static void GoToLine()
        {
            var existingWindow = Application.Current.Windows.OfType<GoToLineWindow>().FirstOrDefault();
            if (existingWindow is not null)
            {
                existingWindow.Activate();
                if (existingWindow.WindowState == WindowState.Minimized)
                {
                    existingWindow.WindowState = WindowState.Normal;
                }
                return;
            }

            var dialog = new GoToLineWindow
            {
                Owner = Application.Current.MainWindow
            };
            dialog.Show();
        }

        [RelayCommand]
        private static void Replace()
        {
            var existingWindow = Application.Current.Windows.OfType<ReplaceWindow>().FirstOrDefault();
            if (existingWindow is not null)
            {
                existingWindow.Activate();
                if (existingWindow.WindowState == WindowState.Minimized)
                {
                    existingWindow.WindowState = WindowState.Normal;
                }
                return;
            }

            var dialog = new ReplaceWindow
            {
                Owner = Application.Current.MainWindow
            };
            dialog.Show();
        }

        [RelayCommand]
        private void Undo()
        {
            if (!_commandStack.CanUndo) return;
            var command = _commandStack.UndoPop();
            command.Rollback();
        }

        [RelayCommand]
        private void Redo()
        {
            if (!_commandStack.CanRedo) return;
            var command = _commandStack.RedoPop();
            command.Commit();
        }

        [RelayCommand]
        private async Task SetScriptLocation()
        {
            if (!_dataService.IsProjectDataLoaded)
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

            try
            {
                await SetScriptLocationTask(folderName);
                MessageBox.Show("Script location set successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set script location. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetScriptLocationTask(string folderName)
        {
            await UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.ScriptFolder, folderName);
            ScriptFolder = folderName;
        }

        [RelayCommand]
        private async Task GenerateScript()
        {
            if (!_dataService.IsProjectDataLoaded)
            {
                MessageBox.Show("No project loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_dataService.IsGameDataLoaded)
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

                    await SetScriptLocationTask(folderName);
                }
                else
                {
                    MessageBox.Show("Script location is required to generate the script.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            LoadingService.Show();
            try
            {
                await GenerateScriptTask(_dataService.ProjectDataFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate script. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadingService.Hide();
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
            await AddVariableFileCommand.Execute(ScriptFolder, new
            (
                 DebugMode ? 0 : DevelopmentMode ? 1 : 2,
                 PrintComments,
                 PrintSavegame,
                 PrintTech
            ));
            await AddInfoFileCommand.Execute(ScriptFolder);

            var getStepQuery = new GetStepsQuery
            {
                ProjectDataFile = projectDataFile,
                Name = "",
            };
            var steps = getStepQuery.Execute();

            var getBuildingsQuery = new GetBuildingsQuery
            {
                ProjectDataFile = projectDataFile
            };
            var buildings = getBuildingsQuery.Execute();

            await AddStepsFileCommand.Execute(ScriptFolder, steps, buildings);
        }

        [RelayCommand]
        private async Task NewProject()
        {
            if (!_dataService.IsGameDataLoaded)
            {
                MessageBox.Show("No game data loaded. Please dump or load game data first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var dialog = new SaveFileDialog
            {
                Filter = "Tas database (*.db)|*.db",
                FileName = "NewProject.db"
            };
            if (dialog.ShowDialog() != true)
                return;

            var filename = dialog.FileName;
            if (string.IsNullOrEmpty(filename))
                return;

            using var context = new ProjectDbContext(filename);
            await Task.Run(context.Database.EnsureCreated);
            await Task.Run(context.SetupTriggers);

            Properties.Settings.Default.ProjectDataFile = filename;
            Properties.Settings.Default.Save();

            _dataService.LoadProjectDataFile();

            MessageBox.Show("New project database created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task DumpData()
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

            LoadingService.Show();

            if (filename.EndsWith("factorio.exe"))
            {
                try
                {
                    await DumpDataTask(filename);
                    MessageBox.Show($"Game data dumped successfully. Version: {_dataService.Version}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

            LoadingService.Hide();
        }

        private async Task DumpDataTask(string filename)
        {
            var version = await DumpFactorioDataCommand.Execute(filename);

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

            var gameDataFile = $"{version}.json";
            await File.WriteAllTextAsync(gameDataFile, JsonSerializer.Serialize(gameData));

            Properties.Settings.Default.GameDataFile = gameDataFile;
            Properties.Settings.Default.Save();

            _dataService.LoadGameDataFile();
        }

        private async Task LoadFactorioDataTask(string filename)
        {
            using var fileContent = File.OpenRead(filename);

            var gameData = await JsonSerializer.DeserializeAsync<GameData>(fileContent);

            Properties.Settings.Default.GameDataFile = Path.GetFileName(filename);
            Properties.Settings.Default.Save();

            _dataService.LoadGameDataFile();
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if (!_dataService.IsGameDataLoaded)
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

            LoadingService.Show();
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
            LoadingService.Hide();
        }

        private async Task MigrateTasFile(string filename)
        {
            var tasFileResult = await ParseTasFileCommand.Execute(filename, _dataService.GameData);

            var dbFile = Path.Combine(Path.GetDirectoryName(filename)!, $"{Path.GetFileNameWithoutExtension(filename)}.db");

            var result = MessageBox.Show($"Tool will create a new db file for this project at {dbFile} (existing file will be overrided) ", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No)
                return;

            await MigrateTasFileDataCommand.Execute(dbFile, tasFileResult);

            PrintComments = tasFileResult.PrintComments;
            PrintSavegame = tasFileResult.PrintSavegame;
            PrintTech = tasFileResult.PrintTech;

            DebugMode = tasFileResult.Environment == 0;
            DevelopmentMode = tasFileResult.Environment == 1;
            ProductionMode = tasFileResult.Environment == 2;
            ScriptFolder = tasFileResult.ScriptFolder;

            Properties.Settings.Default.ProjectDataFile = dbFile;
            Properties.Settings.Default.Save();

            _dataService.LoadProjectDataFile();
        }

        private async Task OpenFileTask(string filename)
        {
            using var context = new ProjectDbContext(filename);
            if (context.Settings.Any())
            {
                Properties.Settings.Default.ProjectDataFile = filename;
                Properties.Settings.Default.Save();

                _dataService.LoadProjectDataFile();
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
            if (!_dataService.IsProjectDataLoaded)
                return;

            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.PrintMessage, value ? "1" : "0").Wait();
        }

        [ObservableProperty]
        private bool _printSavegame = true;

        partial void OnPrintSavegameChanged(bool value)
        {
            if (!_dataService.IsProjectDataLoaded)
                return;

            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.PrintSavegame, value ? "1" : "0").Wait();
        }

        [ObservableProperty]
        private bool _printTech = true;

        partial void OnPrintTechChanged(bool value)
        {
            if (!_dataService.IsProjectDataLoaded)
                return;

            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.PrintTech, value ? "1" : "0").Wait();
        }

        [ObservableProperty]
        private bool _debugMode = false;

        partial void OnDebugModeChanged(bool value)
        {
            if (!value)
                return;
            if (!_dataService.IsProjectDataLoaded)
                return;
            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.Environment, "0").Wait();
        }

        [ObservableProperty]
        private bool _developmentMode = true;

        partial void OnDevelopmentModeChanged(bool value)
        {
            if (!value)
                return;
            if (!_dataService.IsProjectDataLoaded)
                return;

            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.Environment, "1").Wait();
        }

        [ObservableProperty]
        private bool _productionMode = false;

        partial void OnProductionModeChanged(bool value)
        {
            if (!value)
                return;
            if (!_dataService.IsProjectDataLoaded)
                return;

            UpdateSettingCommand.Execute(_dataService.ProjectDataFile, SettingConstants.Environment, "2").Wait();
        }
    }
}