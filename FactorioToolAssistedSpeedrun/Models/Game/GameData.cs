using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class GameData
    {
        public GameData()
        { }

        public GameData(PrototypeData prototypeData)
        {
            Technologies = [.. prototypeData.Technologies
                .Select(t => new TechnologyGame(t.Value))];
            Items = [.. prototypeData.Items
                .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                .Select(i => new ItemGame(i.Value))];
            Recipes = [.. prototypeData.Recipes
                .Where(r => !r.Value.Hidden)
                .Select(r => new RecipeGame(r.Value))];
        }

        public List<TechnologyGame>? Technologies { get; set; }

        public List<ItemGame>? Items { get; set; }

        public List<RecipeGame>? Recipes { get; set; }
    }
}