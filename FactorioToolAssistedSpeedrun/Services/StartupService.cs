using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Models.Game;
using System.IO;
using System.Text.Json;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class StartupService : ObservableObject
    {
        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "Not loaded";

        public GameData? GameData { get; private set; }
        public string ProjectDataFile { get; private set; } = "";

        public bool IsGameDataLoaded { get; private set; }

        public bool IsProjectDataLoaded { get; private set; }

        public event Action? OnGameDataLoaded;

        public event Action? OnProjectDataLoaded;

        public bool LoadGameDataFile()
        {
            IsGameDataLoaded = false;
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!File.Exists(gameDataFile))
                return false;

            var fileContent = File.ReadAllText(gameDataFile);
            GameData = JsonSerializer.Deserialize<GameData>(fileContent);

            App.Current.Dispatcher.Invoke(() => Version = Path.GetFileNameWithoutExtension(gameDataFile));

            IsGameDataLoaded = true;
            OnGameDataLoaded?.Invoke();
            return true;
        }

        public bool LoadProjectDataFile()
        {
            IsProjectDataLoaded = false;
            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!File.Exists(projectDataFile))
                return false;

            ProjectDataFile = projectDataFile;
            App.Current.Dispatcher.Invoke(() => ProjectName = Path.GetFileNameWithoutExtension(projectDataFile));
            OnProjectDataLoaded?.Invoke();
            return true;
        }
    }
}