using FactorioToolAssistedSpeedrun.Models.Prototypes;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class ItemGame(ItemPrototype prototype) : DataBase(prototype)
    {
        public int StackSize { get; set; } = prototype.StackSize;
        public bool IsBuilable { get; set; } = prototype.PlaceAsTile is not null || prototype.PlaceResult is not null;
    }
}