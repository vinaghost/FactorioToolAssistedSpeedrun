using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class ParameterFlagExtensions
    {
        private static readonly FrozenDictionary<StepType, ParameterFlag> _lookup = new Dictionary<StepType, ParameterFlag>
        {
            { StepType.Stop, ParameterFlag.None },
            { StepType.Build, ParameterFlag.Point | ParameterFlag.Item | ParameterFlag.Orientation },
            { StepType.Craft, ParameterFlag.Item | ParameterFlag.Amount },
            { StepType.Speed, ParameterFlag.Amount },
            { StepType.Pause, ParameterFlag.None },
            { StepType.Save, ParameterFlag.None },
            { StepType.Recipe, ParameterFlag.Point | ParameterFlag.Item },
            { StepType.Limit, ParameterFlag.Point | ParameterFlag.Amount | ParameterFlag.Inventory },
            { StepType.Filter, ParameterFlag.Point | ParameterFlag.Item | ParameterFlag.Amount },
            { StepType.Rotate, ParameterFlag.Point | ParameterFlag.Amount },
            { StepType.Priority, ParameterFlag.Point | ParameterFlag.Modifier },
            { StepType.Put, ParameterFlag.Point | ParameterFlag.Item | ParameterFlag.Amount | ParameterFlag.Inventory },
            { StepType.Take, ParameterFlag.Point | ParameterFlag.Item | ParameterFlag.Amount | ParameterFlag.Inventory | ParameterFlag.Modifier },
            { StepType.Mine, ParameterFlag.Point | ParameterFlag.Amount | ParameterFlag.Modifier },
            { StepType.Launch, ParameterFlag.Point },
            { StepType.Next, ParameterFlag.None },
            { StepType.Walk, ParameterFlag.Point },
            { StepType.Tech, ParameterFlag.Item },
            { StepType.Drop, ParameterFlag.Point | ParameterFlag.Item },
            { StepType.PickUp, ParameterFlag.Amount },
            { StepType.Wait, ParameterFlag.Amount },
            { StepType.CancelCrafting, ParameterFlag.Item | ParameterFlag.Amount },
            { StepType.NeverIdle, ParameterFlag.None },
            { StepType.KeepWalking, ParameterFlag.None },
            { StepType.KeepOnPath, ParameterFlag.None },
            { StepType.KeepCrafting, ParameterFlag.None },
            { StepType.Shoot, ParameterFlag.Point | ParameterFlag.Amount },
            { StepType.Equip, ParameterFlag.Item | ParameterFlag.Amount | ParameterFlag.Inventory },
            { StepType.Throw, ParameterFlag.Point | ParameterFlag.Item },
            { StepType.Enter, ParameterFlag.None },
            { StepType.Drive, ParameterFlag.None },
            { StepType.Send, ParameterFlag.None }
        }.ToFrozenDictionary();

        public static bool ContainFlag(this StepType stepType, ParameterFlag flag)
        {
            if (flag == ParameterFlag.None)
            {
                throw new ArgumentException("Cannot check for None flag.", nameof(flag));
            }
            if (_lookup.TryGetValue(stepType, out var parameterFlags))
            {
                return (parameterFlags & flag) == flag;
            }
            return false;
        }
    }
}