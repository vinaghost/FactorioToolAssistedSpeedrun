using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class DeleteStepCommand : IStepCommand
    {
        public required List<Step> Steps { get; init; }

        public void Commit()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            context.Steps
                .Where(x => Steps.Select(s => s.Id).Contains(x.Id))
                .ExecuteDelete();
            context.Steps
                .Where(x => x.Location > Steps.Max(s => s.Location))
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location - Steps.Count));
        }

        public void Commit(ObservableCollection<StepModel> steps)
        {
            Commit();
            foreach (var location in Steps.OrderByDescending(x => x.Location).Select(x => x.Location - 1))
            {
                steps.RemoveAt(location);
            }

            var maxLocation = Steps.Max(x => x.Location);
            foreach (var step in steps.Where(x => x.Location > maxLocation))
            {
                step.Location -= Steps.Count;
            }
        }

        public void Rollback()
        {
            using var context = new ProjectDbContext(App.Current.ProjectDataFile!);
            context.Steps
                .Where(x => x.Location >= Steps.Min(s => s.Location))
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location + Steps.Count));
            context.Steps.AddRange(Steps);
            context.SaveChanges();
        }

        public void Rollback(ObservableCollection<StepModel> steps)
        {
            Rollback();
            var minLocation = Steps.Min(x => x.Location);
            foreach (var step in steps.Where(x => x.Location >= minLocation))
            {
                step.Location += Steps.Count;
            }
            foreach (var step in Steps.OrderByDescending(x => x.Location))
            {
                var model = new StepModel();
                model.FromEntity(step);
                steps.Insert(minLocation - 1, model);
            }
        }
    }
}