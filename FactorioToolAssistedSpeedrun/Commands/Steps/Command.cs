using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record CommandParameters(string Name);

    public abstract class Command<T> : ICommand, IUICommand, IDatabaseCommand where T : CommandParameters
    {
        protected readonly IDataService _dataService;
        protected readonly PanelService _panelService;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public Command(IDataService dataService, PanelService panelService)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _dataService = dataService;
            _panelService = panelService;
        }

        public void Setup(T parameters)
        {
            Parameters = parameters;
        }

        public T Parameters { get; private set; }

        public void Commit(bool ignoreUI = false)
        {
            if (!_dataService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(_dataService.ProjectDataFile!))
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
                    _panelService.StepCollection.SupressNotifications();
                    UICommit(_panelService.StepCollection);
                    _panelService.StepCollection.ResumeNotifications();
                }
                else
                {
                    if (Parameters.Name == _panelService.SelectedTemplate)
                    {
                        _panelService.TemplateStepCollection.SupressNotifications();
                        UICommit(_panelService.TemplateStepCollection);
                        _panelService.TemplateStepCollection.ResumeNotifications();
                    }
                }
            }
        }

        public void Rollback()
        {
            if (!_dataService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(_dataService.ProjectDataFile!))
            {
                DatabaseRollback(context);
            }

            if (Parameters.Name == "")
            {
                _panelService.StepCollection.SupressNotifications();
                UIRollback(_panelService.StepCollection);
                _panelService.StepCollection.ResumeNotifications();
            }
            else
            {
                if (Parameters.Name == _panelService.SelectedTemplate)
                {
                    _panelService.TemplateStepCollection.SupressNotifications();
                    UIRollback(_panelService.TemplateStepCollection);
                    _panelService.TemplateStepCollection.ResumeNotifications();
                }
            }
        }

        public abstract void DatabaseCommit(ProjectDbContext context);

        public abstract void DatabaseRollback(ProjectDbContext context);

        public abstract void UICommit(ObservableCollection<StepModel> collection);

        public abstract void UIRollback(ObservableCollection<StepModel> collection);
    }
}