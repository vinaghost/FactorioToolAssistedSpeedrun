using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class StepTypeExtensions
    {
        private static readonly FrozenDictionary<string, StepType> _lookup = new Dictionary<string, StepType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Stop", StepType.Stop },
            { "Build", StepType.Build },
            { "Craft", StepType.Craft },
            { "Game speed", StepType.Speed },
            { "Pause", StepType.Pause },
            { "Save", StepType.Save },
            { "Recipe", StepType.Recipe },
            { "Limit", StepType.Limit },
            { "Filter", StepType.Filter },
            { "Rotate", StepType.Rotate },
            { "Priority", StepType.Priority },
            { "Put", StepType.Put },
            { "Take", StepType.Take },
            { "Mine", StepType.Mine },
            { "Launch", StepType.Launch },
            { "Next", StepType.Next },
            { "Walk", StepType.Walk },
            { "Tech", StepType.Tech },
            { "Drop", StepType.Drop },
            { "Pick up", StepType.PickUp },
            { "Wait", StepType.Wait },
            { "Cancel crafting", StepType.CancelCrafting },
            { "Never idle", StepType.NeverIdle },
            { "Keep walking", StepType.KeepWalking },
            { "Keep on path", StepType.KeepOnPath },
            { "Keep crafting", StepType.KeepCrafting },
            { "Shoot", StepType.Shoot },
            { "Equip", StepType.Equip },
            { "Throw", StepType.Throw },
            { "Enter", StepType.Enter },
            { "Drive", StepType.Drive },
            { "Send", StepType.Send }
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<StepType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static StepType ToStepType(this string type)
        {
            if (_lookup.TryGetValue(type.Trim(), out var stepType))
                return stepType;
            return StepType.Stop;
        }

        public static string ToStepTypeString(this StepType stepType)
        {
            if (_reverseLookup.TryGetValue(stepType, out var str))
                return str;
            return _reverseLookup[StepType.Stop];
        }

        public static readonly FrozenSet<StepType> StepTypeHasCoordinate = new List<StepType>
        {
            StepType.Build,
            StepType.Recipe,
            StepType.Limit,
            StepType.Filter,
            StepType.Rotate,
            StepType.Shoot,
            StepType.Priority,
            StepType.Take,
            StepType.Put,
            StepType.Mine,
            StepType.Launch,
            StepType.Walk,
            StepType.Drop,
            StepType.Throw
        }.ToFrozenSet();

        public static readonly FrozenSet<StepType> StepTypeHasItemName = new List<StepType>
        {
            StepType.Build,
            StepType.Craft,
            StepType.Filter,
            StepType.Take,
            StepType.Put,
            StepType.Drop,
            StepType.Throw,
            StepType.Equip,
            StepType.CancelCrafting
        }.ToFrozenSet();

        public static readonly FrozenSet<StepType> StepTypeHasAmount = new List<StepType>
        {
            StepType.Craft,
            StepType.Speed,
            StepType.Limit,
            StepType.Filter,
            StepType.Rotate,
            StepType.Shoot,
            StepType.Take,
            StepType.Put,
            StepType.Mine,
            StepType.Walk,
            StepType.PickUp,
            StepType.Wait,
            StepType.Equip,
            StepType.CancelCrafting
        }.ToFrozenSet();

        public static readonly FrozenSet<StepType> StepTypeHasModifier = new List<StepType>
        {
            StepType.Mine,
            StepType.Take,
            StepType.Wait,
        }.ToFrozenSet();
    }
}