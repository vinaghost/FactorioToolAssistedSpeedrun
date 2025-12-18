using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Humanizer;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class DataBase(PrototypeBase prototype)
    {
        public string Name { get; set; } = prototype.Name;
        public string HumanizeName { get; set; } = prototype.Name.Humanize();
    }
}