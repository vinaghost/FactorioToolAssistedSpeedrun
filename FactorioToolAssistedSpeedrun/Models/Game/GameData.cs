using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class GameData(PrototypeData prototypeData)
    {
        public List<TechnologyGame> Technologies { get; set; } =
            [.. prototypeData.Technologies
                .Select(t => new TechnologyGame(t.Value))];

        public List<ItemGame> Items { get; set; } =
            [.. prototypeData.Items
                .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                .Select(i => new ItemGame(i.Value))];

        public List<RecipeGame> Recipes { get; set; } =
            [.. prototypeData.Recipes
                .Where(r => !r.Value.Hidden)
                .Select(r => new RecipeGame(r.Value))];
    }
}