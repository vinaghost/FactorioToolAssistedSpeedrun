using CommunityToolkit.Mvvm.ComponentModel;
using FactorioToolAssistedSpeedrun.Commands.UI;
using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Queries;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class PanelService : ObservableObject
    {
        private readonly IStartupService _startupService;

        public PanelService(IStartupService startupService)
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

            var getSelectedRowQuery = new GetSelectedRowQuery
            {
                ProjectDataFile = _startupService.ProjectDataFile
            };
            var row = getSelectedRowQuery.Execute() + 1;

            var getTemplatesQuery = new GetTemplatesQuery()
            {
                ProjectDataFile = _startupService.ProjectDataFile,
            };
            var templates = getTemplatesQuery.Execute();

            var getCraftingStepQUery = new GetCraftingStepQuery()
            {
                ProjectDataFile = _startupService.ProjectDataFile
            };

            var crafts = getCraftingStepQUery.Execute();
            App.Current.Dispatcher.Invoke(() =>
            {
                LoadSteps(steps);

                TemplateCollection.Clear();
                foreach (var template in templates)
                {
                    TemplateCollection.Add(template);
                }
                if (TemplateCollection.Count > 0)
                    SelectedTemplate = TemplateCollection[0];

                ScrollTo(row);

                LoadCrafting(crafts);
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

        partial void OnSelectedStepIndexChanged(int value)
        {
            UpdateSettingCommand.Execute(_startupService.ProjectDataFile, SettingConstants.SelectedRow, value.ToString()).Wait();
        }

        public ObservableCollection<CraftingModel> CraftingCollection { get; set; } = [];

        [ObservableProperty]
        private CraftingModel? _selectedCraftStep;

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
            if (line < 1 || line > StepCollection.Count)
            {
                return;
            }
            var center = Math.Min(StepCollection.Count - 1, line + 20);
            SelectedStep = StepCollection[center];

            ScrollToSelectedStep?.Invoke();

            if (center != line - 1)
                SelectedStep = StepCollection[line - 1];
        }

        public void LoadCraft()
        {
            if (!_startupService.IsProjectDataLoaded)
            {
                App.Current.Dispatcher.Invoke(() => LoadCrafting([]));
                return;
            }
            var getCraftingStepQUery = new GetCraftingStepQuery()
            {
                ProjectDataFile = _startupService.ProjectDataFile
            };
            var crafts = getCraftingStepQUery.Execute();

            var currentItem = SelectedCraftStep?.Id ?? Guid.Empty;
            App.Current.Dispatcher.Invoke(() =>
            {
                LoadCrafting(crafts);
                if (currentItem != Guid.Empty)
                {
                    SelectedCraftStep = CraftingCollection.FirstOrDefault(x => x.Id == currentItem);
                }
                ;
            });
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

        private void LoadCrafting(List<Step> steps)
        {
            var collection = CraftingCollection;
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
                    CraftingModel model = new();
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

        private void LoadSteps(List<Step> steps, bool template = false)
        {
            var collection = template ? TemplateStepCollection : StepCollection;
            if (!template)
            {
                StepsChangeStarted?.Invoke();
            }
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

            if (!template)
            {
                StepsChangeCompleted?.Invoke();
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