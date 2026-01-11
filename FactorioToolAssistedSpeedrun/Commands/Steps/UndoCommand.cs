using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public abstract class UndoCommand : IUndoCommand
    {
        public required ObservableCollection<StepModel> Collection { get; init; }

        protected abstract void DatabaseCommit(ProjectDbContext context);

        protected abstract void UICommit(ObservableCollection<StepModel> collection);

        public void Commit()
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

            UICommit(Collection);
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

            UIRollback(Collection);
        }
    }
}