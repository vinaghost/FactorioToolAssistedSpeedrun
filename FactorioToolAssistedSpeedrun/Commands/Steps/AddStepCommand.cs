using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    internal class AddStepCommand : UndoCommand
    {
        public required List<Step> Steps { get; init; }

        protected override void DatabaseCommit(ProjectDbContext context)
        {
            var minLocation = Steps.Min(x => x.Location);
            context.Steps
                .Where(x => x.Location >= minLocation && x.Name == Name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location + Steps.Count));
            context.Steps.AddRange(Steps);
            context.SaveChanges();
        }

        protected override void UICommit(ObservableCollection<StepModel> collection)
        {
            var minLocation = Steps.Min(x => x.Location);
            foreach (var step in collection.Where(x => x.Location >= minLocation))
            {
                step.Location += Steps.Count;
            }
            foreach (var step in Steps.OrderByDescending(x => x.Location))
            {
                var model = new StepModel();
                model.FromEntity(step);
                collection.Insert(minLocation - 1, model);
            }
        }

        protected override void DatabaseRollback(ProjectDbContext context)
        {
            context.Steps
                .Where(x => Steps.Select(s => s.Id).Contains(x.Id) && x.Name == Name)
                .ExecuteDelete();
            var maxLocation = Steps.Max(x => x.Location);
            context.Steps
                .Where(x => x.Location > maxLocation && x.Name == Name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location - Steps.Count));
        }

        protected override void UIRollback(ObservableCollection<StepModel> collection)
        {
            if (collection.Count == 0) return;
            var name = collection.First().Name;
            if (!string.IsNullOrEmpty(name) && name != Name)
                return;

            foreach (var location in Steps.OrderByDescending(x => x.Location).Select(x => x.Location - 1))
            {
                collection.RemoveAt(location);
            }

            var maxLocation = Steps.Max(x => x.Location);
            foreach (var step in collection.Where(x => x.Location > maxLocation))
            {
                step.Location -= Steps.Count;
            }
        }
    }
}