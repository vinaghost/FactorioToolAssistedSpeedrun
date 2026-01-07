using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class UpdateStepPropertyCommand : IStepCommand
    {
        public required Step OldSteps { get; init; }
        public required Step NewSteps { get; init; }

        public void Commit()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            context.Steps.Update(NewSteps);
            context.Entry(NewSteps).Property(x => x.Type).IsModified = false;
            context.SaveChanges();
        }

        public void Commit(ObservableCollection<StepModel> steps)
        {
            Commit();

            var currentStepModel = steps.FirstOrDefault(s => s.Id == NewSteps.Id);
            currentStepModel?.FromEntity(NewSteps);
        }

        public void Rollback()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            context.Steps.Update(OldSteps);
            context.Entry(OldSteps).Property(x => x.Type).IsModified = false;
            context.SaveChanges();
        }

        public void Rollback(ObservableCollection<StepModel> steps)
        {
            Rollback();
            var currentStepModel = steps.FirstOrDefault(s => s.Id == OldSteps.Id);
            currentStepModel?.FromEntity(OldSteps);
        }
    }
}