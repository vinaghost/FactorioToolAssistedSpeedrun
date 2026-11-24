using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record DeleteStepCommandParameters(string Name, List<Step> Steps) : CommandParameters(Name);

    public class DeleteStepCommand : Command<DeleteStepCommandParameters>
    {
        public DeleteStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.DeleteSteps(name, steps);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.DeleteSteps(steps);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps) = Parameters;
            context.AddSteps(name, steps);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps) = Parameters;
            collection.AddSteps(steps);
        }
    }

    public static class DeleteStepCommandExtension
    {
        public static void DeleteSteps(this ObservableCollection<StepModel> collection, List<Step> steps)
        {
            if (steps.Count == 0) return;

            var ids = steps.Select(x => x.Id).ToHashSet();
            var indicesToRemove = collection
                .Select((step, index) => (step, index))
                .Where(x => ids.Contains(x.step.Id))
                .Select(x => x.index)
                .OrderByDescending(index => index)
                .ToList();

            if (indicesToRemove.Count == 0) return;

            foreach (var index in indicesToRemove)
            {
                collection.RemoveAt(index);
            }

            var maxLocation = steps.Max(x => x.Location);
            foreach (var step in collection.Where(x => x.Location > maxLocation))
            {
                step.Location -= indicesToRemove.Count;
            }
        }

        public static void DeleteSteps(this ProjectDbContext context, string name, List<Step> steps)
        {
            if (steps.Count == 0) return;

            var idClause = ProjectDbContext.IdInClause(steps.Select(s => s.Id));
            var maxLocation = steps.Max(x => x.Location);

            context.RunInTransaction(() =>
            {
                context.ExecuteNonQuery(
                    $"DELETE FROM Steps WHERE Name = $name AND Id IN ({idClause})",
                    new SqliteParameter("$name", name));

                context.ExecuteNonQuery(
                    "UPDATE Steps SET Location = Location - $count WHERE Location > $maxLocation AND Name = $name",
                    new SqliteParameter("$count", steps.Count),
                    new SqliteParameter("$maxLocation", maxLocation),
                    new SqliteParameter("$name", name));
            });
        }
    }
}