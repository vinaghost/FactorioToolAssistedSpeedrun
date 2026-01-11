using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class UpdateStepPropertyCommand : UndoCommand
    {
        public required Step OldSteps { get; init; }
        public required Step NewSteps { get; init; }

        protected override void DatabaseCommit(ProjectDbContext context)
        {
            context.Steps.Update(NewSteps);
            context.Entry(NewSteps).Property(x => x.Type).IsModified = false;
            context.SaveChanges();
        }

        protected override void UICommit(ObservableCollection<StepModel> collection)
        {
            var currentStepModel = collection.FirstOrDefault(s => s.Id == NewSteps.Id);
            currentStepModel?.FromEntity(NewSteps);
        }

        protected override void DatabaseRollback(ProjectDbContext context)
        {
            context.Steps.Update(OldSteps);
            context.Entry(OldSteps).Property(x => x.Type).IsModified = false;
            context.SaveChanges();
        }

        protected override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var currentStepModel = Collection.FirstOrDefault(s => s.Id == OldSteps.Id);
            currentStepModel?.FromEntity(OldSteps);
        }
    }
}