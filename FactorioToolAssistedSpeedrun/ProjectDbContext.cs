using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun
{
    public class ProjectDbContext(string name) : DbContext
    {
        public DbSet<Step> Steps { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite($"Data Source={name}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Step>(entityBuilder =>
            {
                entityBuilder.Property(e => e.Priority)
                    .HasConversion(
                        v => Priority.ToString(v),
                        v => Priority.FromString(v));

                entityBuilder.Property(e => e.Inventory)
                    .HasConversion(
                        v => InventoryTypeExtensions.ToString(v),
                        v => InventoryTypeExtensions.FromString(v));

                entityBuilder.Property(e => e.Orientation)
                    .HasConversion(
                        v => OrientationTypeExtensions.ToString(v),
                        v => OrientationTypeExtensions.FromString(v));
                entityBuilder.Property(e => e.Type)
                    .HasConversion(
                        v => StepTypeExtensions.ToString(v),
                        v => StepTypeExtensions.FromString(v));

                entityBuilder.Property(e => e.Modifier)
                    .HasConversion(
                        v => ModifierTypeExtensions.ToString(v),
                        v => ModifierTypeExtensions.FromString(v));
            });
        }

        public void SetupTriggers()
        {
            // Trigger to prevent updating Templates.Type
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS block_update_step_type
BEFORE UPDATE OF TYPE ON Steps
BEGIN
  SELECT RAISE(ABORT, 'Updating Steps.Type is prohibited');
END;
");
        }
    }
}