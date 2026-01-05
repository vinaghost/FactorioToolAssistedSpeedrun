using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.Game;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class StepPanelViewModel : ObservableObject
    {
        private readonly StepTypePanelViewModel _stepTypePanelViewModel;
        private readonly StepDetailPanelViewModel _stepDetailPanelViewModel;

        public StepPanelViewModel()
        {
            _stepDetailPanelViewModel = App.Current.Services.GetRequiredService<StepDetailPanelViewModel>();
            _stepTypePanelViewModel = App.Current.Services.GetRequiredService<StepTypePanelViewModel>();
        }

        [ActivatorUtilitiesConstructor]
        public StepPanelViewModel(StepTypePanelViewModel stepTypePanelViewModel, StepDetailPanelViewModel stepDetailPanelViewModel)
        {
            _stepTypePanelViewModel = stepTypePanelViewModel;
            _stepDetailPanelViewModel = stepDetailPanelViewModel;
        }

        [ObservableProperty]
        private StepModel? _selectedItem;

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];
        public List<string> ItemCollection { get; set; } = [];

        public Action? StepsChangeStarted;

        public Action? StepsChangeCompleted;
        public Action? ScrollToSelected;

        [RelayCommand]
        public async Task MouseRightButtonUp(DataGridRow row)
        {
            var index = row.GetIndex();
            var step = StepCollection[index];

            _stepTypePanelViewModel.SelectedStepType = step.Type;
            _stepDetailPanelViewModel.Load(step);
        }

        public void LoadItems(GameData gameData)
        {
            ItemCollection.Clear();

            ItemCollection.AddRange(gameData.Items.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Recipes.Select(x => x.Key));
            ItemCollection.AddRange(gameData.Technologies.Select(x => x.Key));
        }

        public void LoadSteps(List<Step> steps)
        {
            StepsChangeStarted?.Invoke();

            if (steps.Count < StepCollection.Count)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                while (StepCollection.Count > steps.Count)
                {
                    StepCollection.RemoveAt(StepCollection.Count - 1);
                }
            }
            else if (steps.Count > StepCollection.Count)
            {
                for (int i = 0; i < StepCollection.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
                for (int i = StepCollection.Count; i < steps.Count; i++)
                {
                    StepModel model = new();
                    model.FromEntity(steps[i]);
                    StepCollection.Add(model);
                }
            }
            else
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    StepCollection[i].FromEntity(steps[i]);
                }
            }
            StepsChangeCompleted?.Invoke();
        }
    }
}