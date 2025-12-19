using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class ItemGame : DataBase
    {
        public ItemGame()
        { }

        public ItemGame(ItemPrototype prototype) : base(prototype)
        {
            StackSize = prototype.StackSize;
            IsBuilable = prototype.PlaceAsTile is not null || prototype.PlaceResult is not null;
        }

        public int StackSize { get; set; }
        public bool IsBuilable { get; set; }
    }
}