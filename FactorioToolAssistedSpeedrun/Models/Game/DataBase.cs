using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Humanizer;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class DataBase
    {
        public DataBase()
        { }

        public static Dictionary<string, string> NameDict = new() {
            { "long-handed-inserter", "Long-handed inserter" },
            { "research-speed-1", "Lab research speed 1" },
            { "research-speed-2", "Lab research speed 2" },
        };

        public DataBase(PrototypeBase prototype)
        {
            Name = prototype.Name;

            if (NameDict.TryGetValue(prototype.Name, out string? value))
            {
                HumanizeName = value;
            }
            else
            {
                HumanizeName = prototype.Name.Humanize().Transform(To.SentenceCase);
            }
        }

        public string? Name { get; set; }
        public string? HumanizeName { get; set; }
    }
}