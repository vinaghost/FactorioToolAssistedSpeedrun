using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        [ObservableProperty]
        private int _x;

        [ObservableProperty]
        private int _y;

        [ObservableProperty]
        private int _offset;

        [ObservableProperty]
        private int _multipler = 1;

        partial void OnMultiplerChanged(int value)
        {
            if (value < 1)
            {
                Multipler = 1;
            }
        }

        [ObservableProperty]
        private int _iterator;

        [ObservableProperty]
        private TemplateDirectionType _templateDirection;

        public ObservableCollection<TemplateDirectionType> TemplateDirections { get; set; } =
        [
            TemplateDirectionType.Normal,
            TemplateDirectionType.Left,
            TemplateDirectionType.Reverse,
            TemplateDirectionType.Right,
        ];

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
                App.Current.Dispatcher.Invoke(() => LoadSteps([], true));
                return;
            }

            var getStepsQuery = new GetStepsQuery()
            {
                Name = templateName,
                ProjectDataFile = _startupService.ProjectDataFile!,
            };

            var steps = getStepsQuery.Execute();

            App.Current.Dispatcher.Invoke(() => LoadSteps(steps, true));
        }

        public void AddTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return;
            }
            if (!TemplateCollection.Contains(templateName))
            {
                TemplateCollection.Add(templateName);
                SelectedTemplate = templateName;
            }
        }

        public void RemoveTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return;
            }
            TemplateCollection.Remove(templateName);
            if (SelectedTemplate == templateName)
            {
                SelectedTemplate = TemplateCollection.Count > 0 ? TemplateCollection[0] : null;
            }

            using var context = new ProjectDbContext(_startupService.ProjectDataFile!);
            context.Steps.Where(x => x.Name == templateName).ExecuteDelete();
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

        public void ApplyTemplateModifier(List<Step> steps)
        {
            if (Iterator == 0)
            {
                Iterator = 1;
            }

            var xOffset = X * Iterator;
            var yOffset = Y * Iterator;

            foreach (var step in steps)
            {
                if (step.Type.ContainFlag(ParameterFlag.Amount))
                {
                    step.Amount = step.Amount * Multipler + Offset;
                }
                if (step.Type.ContainFlag(ParameterFlag.Point))
                {
                    var (newX, newY) = Transform(step.X + xOffset, step.Y + yOffset, TemplateDirection);
                    step.X = newX;
                    step.Y = newY;
                }

                if (step.Type.ContainFlag(ParameterFlag.Orientation) && step.Orientation.HasValue)
                {
                    step.Orientation = Transform(step.Orientation.Value, TemplateDirection);
                }
            }
            Iterator++;
        }

        private static (double x, double y) Transform(double x, double y, TemplateDirectionType dir)
        {
            return dir switch
            {
                TemplateDirectionType.Normal => (x, y),
                TemplateDirectionType.Left => (y, -x),
                TemplateDirectionType.Reverse => (-x, -y),
                TemplateDirectionType.Right => (-y, x),
                _ => (x, y),
            };
        }

        private static OrientationType Transform(OrientationType o, TemplateDirectionType dir)
        {
            return dir switch
            {
                TemplateDirectionType.Normal => o,
                TemplateDirectionType.Left => (OrientationType)(((int)o + 3) % 4),
                TemplateDirectionType.Reverse => (OrientationType)(((int)o + 2) % 4),
                TemplateDirectionType.Right => (OrientationType)(((int)o + 1) % 4),
                _ => o,
            };
        }
    }
}