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
            if (Name == "long-handed-inserter")
            {
                HumanizeName = "Long-handed inserter";
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