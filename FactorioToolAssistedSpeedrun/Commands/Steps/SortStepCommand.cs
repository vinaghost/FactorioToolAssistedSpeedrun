using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record SortStepCommandParameters(string Name, List<Step> Steps, SortDirectionType SortDirection, SortFirstType SortFirst) : CommandParameters(Name);

    public class SortStepCommand : Command<SortStepCommandParameters>
    {
        public SortStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        private static List<Step> Sort(List<Step> steps, SortDirectionType sortDirection, SortFirstType sortFirst)
        {
            if (sortFirst == SortFirstType.X)
            {
                return sortDirection switch
                {
                    SortDirectionType.TopLeft => steps
                        .OrderBy(step => step.X) // X ascending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenBy(step => step.Y) // Y ascending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),

                    SortDirectionType.TopRight => steps
                        .OrderByDescending(step => step.X) // X descending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenBy(step => step.Y) // Y ascending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),

                    SortDirectionType.BottomLeft => steps
                        .OrderBy(step => step.X) // X ascending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenByDescending(step => step.Y) // Y descending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),

                    SortDirectionType.BottomRight => steps
                        .OrderByDescending(step => step.X) // X descending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenByDescending(step => step.Y) // Y descending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),

                    _ => steps
                };
            }
            else
            {
                return sortDirection switch
                {
                    SortDirectionType.TopLeft => steps
                        .OrderBy(step => step.Y) // Y ascending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenBy(step => step.X) // X ascending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),
                    SortDirectionType.TopRight => steps
                        .OrderBy(step => step.Y) // Y ascending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenByDescending(step => step.X) // X descending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),
                    SortDirectionType.BottomLeft => steps
                        .OrderByDescending(step => step.Y) // Y descending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenBy(step => step.X) // X ascending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),
                    SortDirectionType.BottomRight => steps
                        .OrderByDescending(step => step.Y) // Y descending
                        .ThenBy(step => step.Type != StepType.Walk)
                        .ThenByDescending(step => step.X) // X descending
                        .ThenBy(step => step.Type != StepType.Build)
                        .ToList(),
                    _ => steps
                };
            }
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps, sortDirection, sortFirst) = Parameters;
            if (steps == null || steps.Count == 0) return;

            var firstStepLocation = steps.Min(x => x.Location);
            var sortedSteps = Sort(steps, sortDirection, sortFirst);

            context.RunInTransaction(() =>
            {
                for (int i = 0; i < sortedSteps.Count; i++)
                {
                    context.ExecuteNonQuery(
                        "UPDATE Steps SET Location = $location WHERE Name = $name AND Id = $id",
                        new SqliteParameter("$location", firstStepLocation + i),
                        new SqliteParameter("$name", name),
                        new SqliteParameter("$id", sortedSteps[i].Id));
                }
            });
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps, sortDirection, sortFirst) = Parameters;
            if (steps.Count == 0) return;

            var firstStepLocation = steps.Min(x => x.Location);
            var sortedSteps = Sort(steps, sortDirection, sortFirst);

            ApplyOrderToCollection(collection, sortedSteps, firstStepLocation);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps, _, _) = Parameters;
            if (steps == null || steps.Count == 0) return;

            context.RunInTransaction(() =>
            {
                foreach (var step in steps)
                {
                    context.ExecuteNonQuery(
                        "UPDATE Steps SET Location = $location WHERE Name = $name AND Id = $id",
                        new SqliteParameter("$location", step.Location),
                        new SqliteParameter("$name", name),
                        new SqliteParameter("$id", step.Id));
                }
            });
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps, _, _) = Parameters;
            if (steps.Count == 0) return;

            var originalOrdered = steps.OrderBy(x => x.Location).ToList();
            var firstStepLocation = originalOrdered[0].Location;

            ApplyOrderToCollection(collection, originalOrdered, firstStepLocation);
        }

        private static void ApplyOrderToCollection(
            ObservableCollection<StepModel> collection,
            List<Step> orderedSteps,
            int startLocation)
        {
            int endLocation = startLocation + orderedSteps.Count - 1;

            for (int i = 0; i < orderedSteps.Count; i++)
            {
                int newLocation = startLocation + i;
                Guid targetId = orderedSteps[i].Id;

                // Find the matching StepModel within the target range
                StepModel? model = null;
                for (int loc = newLocation; loc <= endLocation; loc++)
                {
                    if (collection[loc - 1].Id == targetId)
                    {
                        model = collection[loc - 1];
                        break;
                    }
                }

                if (model == null) continue;

                int oldLocation = model.Location;
                if (oldLocation == newLocation) continue;

                int step = oldLocation < newLocation ? 1 : -1;
                for (int loc = oldLocation + step; loc != newLocation + step; loc += step)
                {
                    collection[loc - 1].Location -= step;
                }

                model.Location = newLocation;
                collection.Move(oldLocation - 1, newLocation - 1);
            }
        }
    }
}