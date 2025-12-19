using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class TechnologyGame : DataBase
    {
        public TechnologyGame() : base()
        { }

        public TechnologyGame(TechnologyPrototype prototype) : base(prototype)
        {
            Count = prototype.Unit?.Count;
            Time = prototype.Unit?.Time;
            Ingredients = prototype.Unit?.Ingredients;
        }

        public int? Count { get; set; }
        public double? Time { get; set; }
        public List<ResearchIngredient>? Ingredients { get; set; }
    }
}