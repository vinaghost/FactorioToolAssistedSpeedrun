using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Models.Game;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class StartupService : ObservableObject, IStartupService
    {
        [ObservableProperty]
        private string _version = "Not loaded";

        [ObservableProperty]
        private string _projectName = "Not loaded";

        public ObservableCollection<string> ItemsCollection { get; set; } = [];

        public GameData? GameData { get; private set; }
        public string ProjectDataFile { get; private set; } = "";

        public bool IsGameDataLoaded { get; private set; }

        public bool IsProjectDataLoaded { get; private set; }

        public event Action? OnGameDataLoaded;

        public event Action? OnProjectDataLoaded;

        public void LoadGameDataFile()
        {
            IsGameDataLoaded = false;
            var gameDataFile = Properties.Settings.Default.GameDataFile;
            if (!File.Exists(gameDataFile))
                return;

            var fileContent = File.ReadAllText(gameDataFile);
            GameData = JsonSerializer.Deserialize<GameData>(fileContent);

            App.Current.Dispatcher.Invoke(() => Version = Path.GetFileNameWithoutExtension(gameDataFile));

            ItemsCollection.Clear();

            var items = GameData!.Items.Select(x => x.Key)
                .Concat(GameData!.Recipes.Select(x => x.Key))
                .Concat(GameData!.Technologies.Select(x => x.Key));

            foreach (var item in items)
            {
                ItemsCollection.Add(item);
            }

            IsGameDataLoaded = true;
            OnGameDataLoaded?.Invoke();
        }

        public void LoadProjectDataFile()
        {
            IsProjectDataLoaded = false;
            var projectDataFile = Properties.Settings.Default.ProjectDataFile;
            if (!File.Exists(projectDataFile))
                return;

            ProjectDataFile = projectDataFile;
            App.Current.Dispatcher.Invoke(() => ProjectName = Path.GetFileNameWithoutExtension(projectDataFile));
            IsProjectDataLoaded = true;
            OnProjectDataLoaded?.Invoke();
        }
    }
}