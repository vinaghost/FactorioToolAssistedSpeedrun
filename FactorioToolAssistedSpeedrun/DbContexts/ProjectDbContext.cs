using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models;
using FactorioToolAssistedSpeedrun.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.DbContexts
{
    public class ProjectDbContext(string name) : DbContext
    {
        public DbSet<Step> Steps { get; set; }
        public DbSet<Template> Templates { get; set; }
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
                entityBuilder.ComplexProperty(e => e.Option, builder =>
                {
                    builder.Property(e => e.Priority)
                        .HasColumnName("Priority")
                        .HasConversion(
                            v => Priority.ToString(v),
                            v => Priority.FromString(v));

                    builder.Property(e => e.Inventory)
                        .HasColumnName("Inventory")
                        .HasConversion(
                            v => v.ToInventoryTypeString(),
                            v => v.ToInventoryType());

                    builder.Property(e => e.Orientation)
                        .HasColumnName("Orientation")
                        .HasConversion(
                            v => v.ToOrientationTypeString(),
                            v => v.ToOrientationType()
                    );
                });
                entityBuilder.Property(e => e.Type)
                    .HasConversion(
                        v => v.ToStepTypeString(),
                        v => v.ToStepType()
                );

                entityBuilder.Property(e => e.Modifier)
                    .HasConversion(
                        v => v.ToModifierTypeString(),
                        v => v.ToModifierType()
                );
            });
        }

        public void SetupTriggers()
        {
            // Trigger to insert a Building after a Build step
            // Ignore transport-belt because some are built for walking,
            // we don't want spam the table with them
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS insert_building_buildstep_after_build_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'Build' AND NEW.IsSkip = 0 AND NEW.Item != 'transport-belt'
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Location
    WHERE X = NEW.X AND Y = NEW.Y AND DestroyStep = -1;

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
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS update_building_destroystep_after_mine_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'Mine' AND NEW.IsSkip = 0 AND NEW.Modifier = 'split'
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Location
    WHERE X = NEW.X AND Y = NEW.Y AND DestroyStep = -1;
END;
");
            // Trigger to decrement step location and adjust building references
            // when a step is deleted
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS decrement_step_location_after_delete
AFTER DELETE ON Steps
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