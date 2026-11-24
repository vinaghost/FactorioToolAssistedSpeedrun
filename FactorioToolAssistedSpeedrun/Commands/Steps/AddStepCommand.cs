using System.Collections.ObjectModel;

using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record AddStepCommandParameters(string Name, List<Step> Steps) : CommandParameters(Name);

    public class AddStepCommand : Command<AddStepCommandParameters>
    {
        public AddStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.AddSteps(name, steps);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.AddSteps(steps);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.DeleteSteps(name, steps);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.DeleteSteps(steps);
        }
    }

    public static class AddStepCommandExtension
    {
        public static void AddSteps(this ObservableCollection<StepModel> collection, List<Step> steps)
        {
            if (steps.Count == 0)
            {
                return;
            }

            var minLocation = steps.Min(x => x.Location);
            foreach (var step in collection.Where(x => x.Location >= minLocation))
            {
                step.Location += steps.Count;
            }

            int insertIndex = Math.Clamp(minLocation - 1, 0, collection.Count);
            foreach (var step in steps.OrderBy(x => x.Location))
            {
                var model = new StepModel();
                model.FromEntity(step);
                collection.Insert(insertIndex++, model);
            }
        }

        public static void AddSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            if (steps.Count == 0)
            {
                return;
            }
            var minLocation = steps.Min(x => x.Location);
            context.RunInTransaction(() =>
            {
                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location + $count WHERE Location >= $minLocation AND Name = $name",
                    new SqliteParameter("$count", steps.Count),
                    new SqliteParameter("$minLocation", minLocation),
                    new SqliteParameter("$name", name));
                context.InsertSteps(steps);
            });
        }
    }
}