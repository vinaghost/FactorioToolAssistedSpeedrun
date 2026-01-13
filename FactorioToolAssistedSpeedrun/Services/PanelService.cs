using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Queries;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class PanelService : ObservableObject
    {
        private readonly StartupService _startupService;

        public PanelService(StartupService startupService)
        {
            _startupService = startupService;

            _startupService.OnProjectDataLoaded += OnProjectDataLoaded;
        }

        private void OnProjectDataLoaded()
        {
            var getStepsQuery = new GetStepsQuery
            {
                Name = "",
                ProjectDataFile = _startupService.ProjectDataFile,
            };
            var steps = getStepsQuery.Execute();
            App.Current.Dispatcher.Invoke(() => LoadSteps(steps));

            var getTemplatesQuery = new GetTemplatesQuery()
            {
                ProjectDataFile = _startupService.ProjectDataFile!,
            };
            var templates = getTemplatesQuery.Execute();

            App.Current.Dispatcher.Invoke(() =>
            {
                TemplateCollection.Clear();
                foreach (var template in templates)
                {
                    TemplateCollection.Add(template);
                }
                if (TemplateCollection.Count > 0)
                    SelectedTemplate = TemplateCollection[0];
            });
        }

        partial void OnSelectedTemplateChanged(string? value)
        {
            LoadTemplateSteps(value);
        }

        public ObservableCollection<StepModel> StepCollection { get; set; } = [];

        [ObservableProperty]
        private StepModel? _selectedStep;

        [ObservableProperty]
        private int _selectedStepIndex;

        public ObservableCollection<StepModel> TemplateStepCollection { get; set; } = [];

        [ObservableProperty]
        private StepModel? _selectedTemplateStep;

        [ObservableProperty]
        private int _selectedTemplateStepIndex;

        public ObservableCollection<string> TemplateCollection { get; set; } = [];

        [ObservableProperty]
        private string? _selectedTemplate;

        public Action? StepsChangeStarted;
        public Action? StepsChangeCompleted;

        public Action? ScrollToSelectedStep;

        public void ScrollTo(int line)
        {
            if (line <= 0 || line >= StepCollection.Count - 1)
            {
                return;
            }
            var center = Math.Min(StepCollection.Count - 1, line + 20);
            SelectedStep = StepCollection[center];

            ScrollToSelectedStep?.Invoke();

            if (center != line - 1)
                SelectedStep = StepCollection[line - 1];
        }

        public void LoadTemplateSteps(string? templateName = "")
        {
            if (string.IsNullOrEmpty(templateName))
            {
                templateName = SelectedTemplate ?? "";
            }

            if (!_startupService.IsProjectDataLoaded || string.IsNullOrEmpty(templateName))
            {
                LoadSteps([], true);
                return;
            }

            var getStepsQuery = new GetStepsQuery()
            {
                Name = templateName,
                ProjectDataFile = _startupService.ProjectDataFile!,
            };

            var steps = getStepsQuery.Execute();
            LoadSteps(steps, true);
        }

        public void LoadSteps(List<Step> steps, bool template = false)
        {
            var collection = template ? TemplateStepCollection : StepCollection;

            if (steps.Count < collection.Count)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    collection[i].FromEntity(steps[i]);
                }
                while (collection.Count > steps.Count)
                {
                    collection.RemoveAt(collection.Count - 1);
                }
            }
            else if (steps.Count > collection.Count)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    collection[i].FromEntity(steps[i]);
                }
                for (int i = collection.Count; i < steps.Count; i++)
                {
                    StepModel model = new();
                    model.FromEntity(steps[i]);
                    collection.Add(model);
                }
            }
            else
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    collection[i].FromEntity(steps[i]);
                }
            }
        }
    }
}