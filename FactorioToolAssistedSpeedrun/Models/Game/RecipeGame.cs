using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class RecipeGame : DataBase
    {
        public RecipeGame() : base()
        { }

        public RecipeGame(RecipePrototype prototype) : base(prototype)
        {
            Category = prototype.Category;
            Input = prototype.Input?.Select(i => new ProductGame(i)).ToList();
            Output = prototype.Output?.Select(o => new ProductGame(o)).ToList();
        }

        public string? Category { get; set; }
        public List<ProductGame>? Input { get; set; }
        public List<ProductGame>? Output { get; set; }
    }

    public class ProductGame
    {
        public ProductGame()
        { }

        public ProductGame(ProductPrototype prototype)
        {
            Type = prototype.Type;
            Name = prototype.Name;
            Amount = prototype.Amount;
        }

        public string? Type { get; set; }
        public string? Name { get; set; }
        public int Amount { get; set; }
    }
}