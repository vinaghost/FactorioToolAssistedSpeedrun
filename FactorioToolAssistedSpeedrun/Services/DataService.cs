using FactorioToolAssistedSpeedrun.Models.Game;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FactorioToolAssistedSpeedrun.Services
{
    public class DataService : IDataService
    {
        public string Version { get; private set; } = "Not loaded";
        public string ProjectName { get; private set; } = "Not loaded";
        public ObservableCollection<string> ItemsCollection { get; } = [];

        private GameData? _gameData;
        public GameData GameData => _gameData ?? GameData.DefaultGameData;
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
            _gameData = JsonSerializer.Deserialize<GameData>(fileContent);
            if (_gameData is null)
                return;

            Version = Path.GetFileNameWithoutExtension(gameDataFile);
            LoadItemsAutoFill(_gameData);

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
            ProjectName = Path.GetFileNameWithoutExtension(projectDataFile);

            IsProjectDataLoaded = true;
            OnProjectDataLoaded?.Invoke();
        }

        private void LoadItemsAutoFill(GameData gameData)
        {
            ItemsCollection.Clear();
            var items = gameData.Items.Select(x => x.Key)
                .Concat(gameData.Recipes.Select(x => x.Key))
                .Concat(gameData.Technologies.Select(x => x.Key));
            foreach (var item in items)
            {
                ItemsCollection.Add(item);
            }
        }
    }
}