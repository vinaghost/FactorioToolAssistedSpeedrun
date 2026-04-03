using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record SortStepCommandParameters(string Name, List<Step> Steps, SortDirectionType SortDirection, SortFirstType SortFirst) : CommandParameters(Name);

    public class SortStepCommand : Command<SortStepCommandParameters>
    {
        public SortStepCommand(IDataService dataService, PanelService panelService)
            : base(dataService, panelService)
        {
        }

        public static List<Step> Sort(List<Step> steps, SortDirectionType sortDirection, SortFirstType sortFirst)
        {
            Func<Step, object> primaryKey = sortFirst == SortFirstType.Y ? step => step.Y : step => step.X;
            Func<Step, object> secondaryKey = sortFirst == SortFirstType.Y ? step => step.X : step => step.Y;

            return sortDirection switch
            {
                SortDirectionType.TopLeft => steps
                    .OrderBy(primaryKey) // Primary key (Y or X)
                    .ThenBy(step => step.Type != StepType.Walk)
                    .ThenBy(secondaryKey) // Secondary key (X or Y)
                    .ThenBy(step => step.Type != StepType.Build)
                    .ToList(),

                SortDirectionType.TopRight => steps
                    .OrderBy(primaryKey) // Primary key (Y or X)
                    .ThenBy(step => step.Type != StepType.Walk)
                    .ThenByDescending(secondaryKey) // Secondary key (X or Y, descending)
                    .ThenBy(step => step.Type != StepType.Build)
                    .ToList(),

                SortDirectionType.BottomLeft => steps
                    .OrderByDescending(primaryKey) // Primary key (Y or X, descending)
                    .ThenBy(step => step.Type != StepType.Walk)
                    .ThenBy(secondaryKey) // Secondary key (X or Y)
                    .ThenBy(step => step.Type != StepType.Build)
                    .ToList(),

                SortDirectionType.BottomRight => steps
                    .OrderByDescending(primaryKey) // Primary key (Y or X, descending)
                    .ThenBy(step => step.Type != StepType.Walk)
                    .ThenByDescending(secondaryKey) // Secondary key (X or Y, descending)
                    .ThenBy(step => step.Type != StepType.Build)
                    .ToList(),

                _ => steps
            };
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, steps, sortDirection, sortFirst) = Parameters;

            var clonedSteps = steps.Select(step => step.Clone()).ToList();
            var firstStepLocation = clonedSteps.Min(x => x.Location);

            clonedSteps = Sort(clonedSteps, sortDirection, sortFirst);

            // Update the location for each cloned step starting from the first step location
            for (int i = 0; i < clonedSteps.Count; i++)
            {
                clonedSteps[i].Location = firstStepLocation + i;
            }

            context.ChangeTracker.Clear();
            context.DeleteSteps(name, steps);
            context.ChangeTracker.Clear();
            context.AddSteps(name, clonedSteps);
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, steps, sortDirection, sortFirst) = Parameters;

            var clonedSteps = steps.Select(step => step.Clone()).ToList();
            var firstStepLocation = clonedSteps.Min(x => x.Location);

            clonedSteps = Sort(clonedSteps, sortDirection, sortFirst);

            // Update the location for each cloned step starting from the first step location
            for (int i = 0; i < clonedSteps.Count; i++)
            {
                clonedSteps[i].Location = firstStepLocation + i;
            }
            collection.DeleteSteps(steps);
            collection.AddSteps(clonedSteps);
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (name, steps, _, _) = Parameters;
            context.ChangeTracker.Clear();
            context.DeleteSteps(name, steps);
            context.ChangeTracker.Clear();
            context.AddSteps(name, steps);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, steps, _, _) = Parameters;

            collection.DeleteSteps(steps);
            collection.AddSteps(steps);
        }
    }
}