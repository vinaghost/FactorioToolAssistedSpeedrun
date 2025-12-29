using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(Name))]
    public class Template
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }

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

        public static Template FromStep(string name, Step step)
        {
            return new Template
            {
                Name = name,
                Location = step.Location,
                Type = step.Type,
                X = step.X,
                Y = step.Y,
                Amount = step.Amount,
                Item = step.Item,
                Option = step.Option,
                Modifier = step.Modifier,
                Color = step.Color,
                Comment = step.Comment
            };
        }
    }
}