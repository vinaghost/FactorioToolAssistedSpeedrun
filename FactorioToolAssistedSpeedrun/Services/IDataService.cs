using FactorioToolAssistedSpeedrun.Models.Game;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Services
{
    public interface IDataService
    {
        string Version { get; }
        string ProjectName { get; }
        bool IsGameDataLoaded { get; }
        bool IsProjectDataLoaded { get; }
        ObservableCollection<string> ItemsCollection { get; }
        string ProjectDataFile { get; }
        GameData GameData { get; }

        event Action? OnGameDataLoaded;

        event Action? OnProjectDataLoaded;

        void LoadGameDataFile();

        void LoadProjectDataFile();
    }
}