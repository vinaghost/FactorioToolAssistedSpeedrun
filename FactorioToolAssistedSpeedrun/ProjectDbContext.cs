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
        public DbSet<Building> Buildings { get; set; }

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
                        v => ModifierTypeExtensions.FromString(v)
                );
            });
        }

        public async Task SetupTriggers()
        {
            // Trigger to prevent updating Templates.Type
            await Database.ExecuteSqlRawAsync(@"
CREATE TRIGGER IF NOT EXISTS block_update_step_type
BEFORE UPDATE OF TYPE ON Steps
BEGIN
  SELECT RAISE(ABORT, 'Updating Steps.Type is prohibited');
END;
");

            // Trigger to insert a Building after a Build step
            // Ignore transport-belt because some are built for walking,
            // we don't want spam the table with them
            await Database.ExecuteSqlRawAsync(@"
CREATE TRIGGER IF NOT EXISTS insert_building_buildstep_after_build_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'build' AND NEW.Name = '' AND NEW.IsSkip = 0 AND NEW.Item != 'transport-belt'
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Location
    WHERE ABS(X - NEW.X) < 0.0001 AND ABS(Y - NEW.Y) < 0.0001 AND DestroyStep = -1;

    INSERT INTO Buildings (X, Y, Name, Orientation, BuildStep, DestroyStep)
    VALUES (
        NEW.X,
        NEW.Y,
        NEW.Item,
        NEW.Orientation,
        NEW.Location,
        -1
    );
END;
");

            // Trigger to update DestroyStep of Buildings after a Mine step
            // Ignore split step because they didn't actually destroy anything yet
            await Database.ExecuteSqlRawAsync(@"
CREATE TRIGGER IF NOT EXISTS update_building_destroystep_after_mine_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'mine' AND NEW.Name = '' AND NEW.IsSkip = 0 AND NEW.Modifier = 'split'
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Location
    WHERE ABS(X - NEW.X) < 0.0001 AND ABS(Y - NEW.Y) < 0.0001 AND DestroyStep = -1;
END;
");
            // Trigger to decrement step location and adjust building references
            // when a step is deleted
            await Database.ExecuteSqlRawAsync(@"
CREATE TRIGGER IF NOT EXISTS decrement_step_location_after_delete
AFTER DELETE ON Steps
WHEN OLD.Name = ''
BEGIN
    UPDATE Steps
    SET Location = Location - 1
    WHERE Location > OLD.Location;

    UPDATE Buildings
    SET BuildStep = BuildStep - 1
    WHERE BuildStep > OLD.Location;

    UPDATE Buildings
    SET DestroyStep = DestroyStep - 1
    WHERE DestroyStep > OLD.Location;
END;
");
        }
    }
}