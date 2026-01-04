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
            Type = prototype.Type;
        }

        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}