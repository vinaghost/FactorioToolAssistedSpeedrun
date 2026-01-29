using FactorioToolAssistedSpeedrun.Models.Game;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Services
{
    public interface IStartupService
    {
        string Version { get; }
        string ProjectName { get; }
        GameData? GameData { get; }
        bool IsGameDataLoaded { get; }
        bool IsProjectDataLoaded { get; }
        ObservableCollection<string> ItemsCollection { get; set; }
        string ProjectDataFile { get; }

        event Action? OnGameDataLoaded;

        event Action? OnProjectDataLoaded;

        void LoadGameDataFile();

        void LoadProjectDataFile();
    }
}