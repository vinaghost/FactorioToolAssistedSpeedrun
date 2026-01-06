using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using FactorioToolAssistedSpeedrun.Models.UI;

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

        public void Rollback(ObservableCollection<StepModel> steps)
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            context.Steps.Update(OldSteps);
            context.Entry(OldSteps).Property(x => x.Type).IsModified = false;
            context.SaveChanges();

            var currentStepModel = steps.FirstOrDefault(s => s.Id == OldSteps.Id);
            currentStepModel?.FromEntity(OldSteps);
        }
    }
}