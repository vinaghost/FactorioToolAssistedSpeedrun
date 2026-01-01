using System.Collections.Frozen;

namespace FactorioToolAssistedSpeedrun.Enums
{
    public static class StepTypeExtensions
    {
        private static readonly FrozenDictionary<string, StepType> _lookup = new Dictionary<string, StepType>()
        {
            { "stop", StepType.Stop },
            { "build", StepType.Build },
            { "craft", StepType.Craft },
            { "game speed", StepType.Speed },
            { "pause", StepType.Pause },
            { "save", StepType.Save },
            { "recipe", StepType.Recipe },
            { "limit", StepType.Limit },
            { "filter", StepType.Filter },
            { "rotate", StepType.Rotate },
            { "priority", StepType.Priority },
            { "put", StepType.Put },
            { "take", StepType.Take },
            { "mine", StepType.Mine },
            { "launch", StepType.Launch },
            { "next", StepType.Next },
            { "walk", StepType.Walk },
            { "tech", StepType.Tech },
            { "drop", StepType.Drop },
            { "pick up", StepType.PickUp },
            { "wait", StepType.Wait },
            { "cancel crafting", StepType.CancelCrafting },
            { "never idle", StepType.NeverIdle },
            { "keep walking", StepType.KeepWalking },
            { "keep on path", StepType.KeepOnPath },
            { "keep crafting", StepType.KeepCrafting },
            { "shoot", StepType.Shoot },
            { "equip", StepType.Equip },
            { "throw", StepType.Throw },
            { "enter", StepType.Enter },
            { "drive", StepType.Drive },
            { "send", StepType.Send }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenDictionary<StepType, string> _reverseLookup = _lookup.ToFrozenDictionary(x => x.Value, x => x.Key);

        public static StepType FromString(string str)
        {
            if (_lookup.TryGetValue(str, out var step))
                return step;
            return StepType.Stop;
        }

        public static string ToString(StepType step)
        {
            if (_reverseLookup.TryGetValue(step, out var str))
                return str;
            return _reverseLookup[StepType.Stop];
        }
    }
}