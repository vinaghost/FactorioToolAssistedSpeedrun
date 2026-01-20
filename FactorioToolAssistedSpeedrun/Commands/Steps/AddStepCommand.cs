using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record AddStepCommandParameters(string Name, List<Step> Steps) : CommandParameters(Name);

    public class AddStepCommand : Command<AddStepCommandParameters>
    {
        public AddStepCommand(StartupService startupService, PanelService panelService)
            : base(startupService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            var minLocation = steps.Min(x => x.Location);
            context.Steps
                .Where(x => x.Location >= minLocation && x.Name == name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location + steps.Count));
            context.Steps.AddRange(steps);
            context.SaveChanges();
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            var minLocation = steps.Min(x => x.Location);
            foreach (var step in collection.Where(x => x.Location >= minLocation))
            {
                step.Location += steps.Count;
            }
            foreach (var step in steps.OrderByDescending(x => x.Location))
            {
                var model = new StepModel();
                model.FromEntity(step);
                collection.Insert(minLocation - 1, model);
            }
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.Steps
                .Where(x => steps.Select(s => s.Id).Contains(x.Id) && x.Name == name)
                .ExecuteDelete();
            var maxLocation = steps.Max(x => x.Location);
            context.Steps
                .Where(x => x.Location > maxLocation && x.Name == name)
                .ExecuteUpdate(x => x.SetProperty(s => s.Location, s => s.Location - steps.Count));
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            foreach (var location in steps.OrderByDescending(x => x.Location).Select(x => x.Location - 1))
            {
                collection.RemoveAt(location);
            }

            var maxLocation = steps.Max(x => x.Location);
            foreach (var step in collection.Where(x => x.Location > maxLocation))
            {
                step.Location -= steps.Count;
            }
        }
    }
}