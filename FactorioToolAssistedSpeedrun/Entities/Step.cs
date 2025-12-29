using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Type))]
    [Index(nameof(Location))]
    public class Step
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Location { get; set; }
        public StepType Type { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string Item { get; set; } = "";

        public Option? Option { get; set; }

        public ModifierType Modifier { get; set; }
        public string Color { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool IsSkip { get; set; } = false;
    }
}