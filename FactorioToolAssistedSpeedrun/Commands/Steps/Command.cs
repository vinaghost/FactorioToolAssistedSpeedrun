using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record CommandParameters(string Name);

    public abstract class Command<T> : ICommand, IUICommand, IDatabaseCommand where T : CommandParameters
    {
        protected readonly StartupService _startupService;
        protected readonly PanelService _panelService;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public Command(StartupService startupService, PanelService panelService)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _startupService = startupService;
            _panelService = panelService;
        }

        public void Setup(T parameters)
        {
            Parameters = parameters;
        }

        public T Parameters { get; private set; }

        public void Commit(bool ignoreUI = false)
        {
            if (!_startupService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(_startupService.ProjectDataFile!))
            {
                try
                {
                    DatabaseCommit(context);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is not null) ex = ex.InnerException;
                    MessageBox.Show($"An error occurred while committing to the database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!ignoreUI)
            {
                if (Parameters.Name == "")
                {
                    UICommit(_panelService.StepCollection);
                }
                else
                {
                    if (Parameters.Name == _panelService.SelectedTemplate)
                    {
                        UICommit(_panelService.TemplateStepCollection);
                    }
                }
            }
        }

        public void Rollback()
        {
            if (!_startupService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(_startupService.ProjectDataFile!))
            {
                DatabaseRollback(context);
            }

            if (Parameters.Name == "")
            {
                UIRollback(_panelService.StepCollection);
            }
            else
            {
                if (Parameters.Name == _panelService.SelectedTemplate)
                {
                    UIRollback(_panelService.TemplateStepCollection);
                }
            }
        }

        public abstract void DatabaseCommit(ProjectDbContext context);

        public abstract void DatabaseRollback(ProjectDbContext context);

        public abstract void UICommit(ObservableCollection<StepModel> collection);

        public abstract void UIRollback(ObservableCollection<StepModel> collection);
    }
}