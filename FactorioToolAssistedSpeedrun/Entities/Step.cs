using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Name), nameof(Location), IsUnique = true)]
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";

        public int Location { get; set; }
        public StepType Type { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string Item { get; set; } = "";
        public OrientationType? Orientation { get; set; }
        public InventoryType? Inventory { get; set; }
        public Priority? Priority { get; set; }
        public ModifierType? Modifier { get; set; }
        public string Color { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool IsSkip { get; set; } = false;
    }
}