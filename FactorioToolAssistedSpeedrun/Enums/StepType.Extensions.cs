using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class StepTypeExtensions
    {
        private static readonly FrozenDictionary<string, StepType> _lookup = new Dictionary<string, StepType>(StringComparer.OrdinalIgnoreCase)
        {
            { "walk", StepType.Walk },
            { "mine", StepType.Mine },
            { "craft", StepType.Craft },
            { "tech", StepType.Tech },
            { "speed", StepType.Speed },
            { "pause", StepType.Pause },
            { "never idle", StepType.NeverIdle },
            { "keep walking", StepType.KeepWalking },
            { "keep on path", StepType.KeepOnPath },
            { "keep crafting", StepType.KeepCrafting },
            { "launch", StepType.Launch },
            { "save", StepType.Save },
            { "wait", StepType.Wait },
            { "pick", StepType.Pick },
            { "rotate", StepType.Rotate },
            { "build", StepType.Build },
            { "take", StepType.Take },
            { "put", StepType.Put },
            { "recipe", StepType.Recipe },
            { "limit", StepType.Limit },
            { "priority", StepType.Priority },
            { "filter", StepType.Filter },
            { "drop", StepType.Drop }
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<StepType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static StepType ToStepType(this string type)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (_lookup.TryGetValue(type.Trim(), out var stepType))
                return stepType;
            throw new ArgumentException($"Unknown step type: {type}", nameof(type));
        }

        public static string ToStepTypeString(this StepType stepType)
        {
            if (_reverseLookup.TryGetValue(stepType, out var str))
                return str;
            throw new ArgumentException($"Unknown StepType enum value: {stepType}", nameof(stepType));
        }
    }
}