using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class RecipeGame(RecipePrototype prototype) : DataBase(prototype)
    {
        public string Category { get; set; } = prototype.Category;
        public List<ProductGame>? Input { get; set; } = prototype.Input?.Select(i => new ProductGame(i)).ToList();
        public List<ProductGame>? Output { get; set; } = prototype.Output?.Select(o => new ProductGame(o)).ToList();
    }

    public class ProductGame(ProductPrototype prototype)
    {
        public string Type { get; set; } = prototype.Type;
        public string Name { get; set; } = prototype.Name;
        public int Amount { get; set; } = prototype.Amount;
    }
}