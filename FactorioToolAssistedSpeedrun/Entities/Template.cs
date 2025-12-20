using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id), nameof(Name))]
    public class Template
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string Item { get; set; } = "";
        public string Orientation { get; set; } = "";
        public string Modifier { get; set; } = "";
        public string Color { get; set; } = "";
        public string Comment { get; set; } = "";
    }
}