using FactorioToolAssistedSpeedrun.DbContexts;
using FactorioToolAssistedSpeedrun.Models.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public class ApplySkipCommand : UndoCommand
    {
        public required List<Guid> StepIds { get; init; }

        protected override void DatabaseCommit(ProjectDbContext context)
        {
            context.Steps
                .Where(x => StepIds.Contains(x.Id) && x.Name == Name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.IsSkip, b => !b.IsSkip));
        }

        protected override void UICommit(ObservableCollection<StepModel> collection)
        {
            var items = collection
                .Where(x => StepIds.Contains(x.Id))
                .ToList();
            foreach (var item in items)
            {
                item.IsSkip = !item.IsSkip;
            }
        }

        protected override void DatabaseRollback(ProjectDbContext context)
        {
            DatabaseCommit(context);
        }

        protected override void UIRollback(ObservableCollection<StepModel> collection)
        {
            UICommit(collection);
        }
    }
}