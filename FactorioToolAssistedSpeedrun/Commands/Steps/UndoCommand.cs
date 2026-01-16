using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public abstract class UndoCommand : IUndoCommand
    {
        public required string Name { get; init; }

        protected abstract void DatabaseCommit(ProjectDbContext context);

        protected abstract void UICommit(ObservableCollection<StepModel> collection);

        public void Commit(bool ignoreUI = false)
        {
            var startupService = App.Current.Services.GetRequiredService<StartupService>();
            if (!startupService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(startupService.ProjectDataFile!))
            {
                DatabaseCommit(context);
            }

            var panelService = App.Current.Services.GetRequiredService<PanelService>();
            if (!ignoreUI)
            {
                if (Name == "")
                {
                    UICommit(panelService.StepCollection);
                }
                else
                {
                    if (Name == panelService.SelectedTemplate)
                    {
                        UICommit(panelService.TemplateStepCollection);
                    }
                }
            }
        }

        protected abstract void DatabaseRollback(ProjectDbContext context);

        protected abstract void UIRollback(ObservableCollection<StepModel> collection);

        public void Rollback()
        {
            var startupService = App.Current.Services.GetRequiredService<StartupService>();
            if (!startupService.IsProjectDataLoaded)
            {
                return;
            }
            using (var context = new ProjectDbContext(startupService.ProjectDataFile!))
            {
                DatabaseRollback(context);
            }

            var panelService = App.Current.Services.GetRequiredService<PanelService>();
            if (Name == "")
            {
                UIRollback(panelService.StepCollection);
            }
            else
            {
                if (Name == panelService.SelectedTemplate)
                {
                    UIRollback(panelService.TemplateStepCollection);
                }
            }
        }
    }
}