using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(X), nameof(Y), nameof(BuildStep))]
    public class Building
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public string Name { get; set; } = "";
        public string Orientation { get; set; } = "";
        public int BuildStep { get; set; } = -1;
        public int DestroyStep { get; set; } = -1;
    }
}