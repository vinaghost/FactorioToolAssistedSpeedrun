using FactorioToolAssistedSpeedrun.Models.Prototypes;
using Humanizer;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class DataBase
    {
        public DataBase()
        { }

        public DataBase(PrototypeBase prototype)
        {
            Name = prototype.Name;
            HumanizeName = prototype.Name.Humanize().Transform(To.SentenceCase);
        }

        public string? Name { get; set; }
        public string? HumanizeName { get; set; }
    }
}