using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public record ApplySkipCommandParameters(string Name, List<Guid> StepIds) : CommandParameters(Name);

    public class ApplySkipCommand : Command<ApplySkipCommandParameters>
    {
        private readonly CommandStack _commandStack;

        public ApplySkipCommand(StartupService startupService, PanelService panelService, CommandStack commandStack)
            : base(startupService, panelService)
        {
            _commandStack = commandStack;
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (name, stepIds) = Parameters;
            context.Steps
                .Where(x => stepIds.Contains(x.Id) && x.Name == name)
                .ExecuteUpdate(setters => setters
                    .SetProperty(b => b.IsSkip, b => !b.IsSkip));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepIds) = Parameters;
            var items = collection
                .Where(x => stepIds.Contains(x.Id))
                .ToList();

            _commandStack.Lock();
            foreach (var item in items)
            {
                item.IsSkip = !item.IsSkip;
            }
            _commandStack.Unlock();
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            DatabaseCommit(context);
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            UICommit(collection);
        }
    }
}