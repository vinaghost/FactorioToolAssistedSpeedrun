using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

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
            var commandStack = App.Current.Services.GetRequiredService<CommandStack>();
            var items = collection
                .Where(x => StepIds.Contains(x.Id))
                .ToList();
            commandStack.Lock();
            foreach (var item in items)
            {
                item.IsSkip = !item.IsSkip;
            }
            commandStack.Unlock();
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