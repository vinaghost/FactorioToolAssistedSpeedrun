using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Type))]
    public class Step
    {
        public int Id { get; set; }
        public required StepType Type { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string Item { get; set; } = "";
        public string Orientation { get; set; } = "";
        public string Modifier { get; set; } = "";
        public string Color { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool IsSkip { get; set; } = false;
        public bool IsSplit { get; set; } = false;
    }
}