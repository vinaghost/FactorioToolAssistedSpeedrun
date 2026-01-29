using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Models.UI;
using FactorioToolAssistedSpeedrun.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace FactorioToolAssistedSpeedrun.Commands.Steps
{
    public static class ExpressionHelper
    {
        private static readonly ConcurrentDictionary<MemberInfo, Delegate> _setterCache = new();

        public static Action<TClass, TValue> GetSetter<TClass, TValue>(Expression<Func<TClass, TValue>> propertySelector)
        {
            // 1. Get the MemberInfo from the selector expression
            if (propertySelector.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Selector must be a member access expression (e.g., x => x.Property).", nameof(propertySelector));
            }

            var member = memberExpression.Member;
            if (_setterCache.TryGetValue(member, out var cachedSetter))
            {
                return (Action<TClass, TValue>)cachedSetter;
            }

            // 2. Define the parameter for the new value
            var valueParameter = Expression.Parameter(typeof(TValue), "value");

            // 3. Create the assignment expression: instance.Property = value
            var assignmentExpression = Expression.Assign(memberExpression, valueParameter);

            // 4. Create the lambda expression (an Action delegate) that takes the instance and the new value
            // The parameters are the original selector's parameter (the instance) and our new value parameter
            var setterLambda = Expression.Lambda<Action<TClass, TValue>>(
                assignmentExpression,
                propertySelector.Parameters[0], // the instance parameter
                valueParameter
            );
            var setter = setterLambda.Compile();

            _setterCache.TryAdd(member, setter);

            return setter;
        }
    }

    public record UpdateStepPropertyCommandParameters<T, TStep>(string Name, Guid StepId, T OldValue, T NewValue,
        Func<T, TStep> StepPropertyTransformer, Expression<Func<Step, TStep>> StepPropertySelector, Expression<Func<StepModel, T>> StepModelPropertySelector) : CommandParameters(Name);

    public class UpdateStepPropertyCommand<T, TStep> : Command<UpdateStepPropertyCommandParameters<T, TStep>>
    {
        private readonly ICommandStack _commandStack;

        public UpdateStepPropertyCommand(IStartupService startupService, PanelService panelService, ICommandStack commandStack) : base(startupService, panelService)
        {
            _commandStack = commandStack;
        }

        public override void DatabaseCommit(ProjectDbContext context)
        {
            var (_, stepId, _, newValue, stepPropertyTransformer, stepPropertySelector, _) = Parameters;
            var stepValue = stepPropertyTransformer(newValue);
            context.Steps
                .Where(x => x.Id == stepId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(stepPropertySelector, stepValue));
        }

        public override void UICommit(ObservableCollection<StepModel> collection)
        {
            var (_, stepId, _, newValue, _, _, stepModelPropertySelector) = Parameters;

            var currentStepModel = collection.FirstOrDefault(s => s.Id == stepId);
            if (currentStepModel is null) return;
            var setter = ExpressionHelper.GetSetter(stepModelPropertySelector);

            _commandStack.Lock();
            setter(currentStepModel, newValue);
            _commandStack.Unlock();
        }

        public override void DatabaseRollback(ProjectDbContext context)
        {
            var (_, stepId, oldValue, _, stepPropertyTransformer, stepPropertySelector, _) = Parameters;
            var stepValue = stepPropertyTransformer(oldValue);
            context.Steps
               .Where(x => x.Id == stepId)
               .ExecuteUpdate(setters => setters
                   .SetProperty(stepPropertySelector, stepValue));
        }

        public override void UIRollback(ObservableCollection<StepModel> collection)
        {
            var (_, stepId, oldValue, _, _, _, stepModelPropertySelector) = Parameters;
            var currentStepModel = collection.FirstOrDefault(s => s.Id == stepId);
            if (currentStepModel is null) return;
            var setter = ExpressionHelper.GetSetter(stepModelPropertySelector);

            _commandStack.Lock();
            setter(currentStepModel, oldValue);
            _commandStack.Unlock();
        }
    }
}