using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class DataBase
    {
        public DataBase()
        { }

        public DataBase(PrototypeBase prototype)
        {
            Name = prototype.Name;
        }

        public string? Name { get; set; }
    }
}