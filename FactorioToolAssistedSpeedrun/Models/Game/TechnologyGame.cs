using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class TechnologyGame(TechnologyPrototype prototype) : DataBase(prototype)
    {
        public int? Count { get; set; } = prototype.Unit?.Count;
        public double? Time { get; set; } = prototype.Unit?.Time;
        public List<ResearchIngredient>? Ingredients { get; set; } = prototype.Unit?.Ingredients;
    }
}