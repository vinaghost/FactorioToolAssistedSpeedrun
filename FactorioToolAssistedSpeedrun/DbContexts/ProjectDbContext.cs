using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
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
            modelBuilder.Entity<Step>()
                .Property(e => e.Type)
                .HasConversion(
                    v => v.ToStepTypeString(),
                    v => v.ToStepType()
                );
        }

        public void SetupTriggers()
        {
            // Trigger to insert a Building after a Build step
            // Ignore transport-belt because some are built for walking,
            // we don't want spam the table with them
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS insert_building_after_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'build' AND NEW.IsSkip = 0 AND NEW.Item != 'transport-belt'
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Id
    WHERE X = NEW.X AND Y = NEW.Y AND DestroyStep = -1;

    INSERT INTO Buildings (X, Y, Name, Orientation, BuildStep, DestroyStep)
    VALUES (
        NEW.X,
        NEW.Y,
        NEW.Item,
        NEW.Orientation,
        NEW.Id,
        -1
    );
END;
");

            // Trigger to update DestroyStep of Buildings after a Mine step
            // Ignore split step because they didn't actually destroy anything yet
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS update_building_destroystep_after_mine_step
AFTER INSERT ON Steps
WHEN NEW.Type = 'mine' AND NEW.IsSkip = 0 AND NEW.IsSplit = 0
BEGIN
    UPDATE Buildings
    SET DestroyStep = NEW.Id
    WHERE X = NEW.X AND Y = NEW.Y AND DestroyStep = -1;
END;
");
            // Trigger to increment step IDs and adjust building references
            // when a new step is inserted
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS increment_step_ids_after_insert
BEFORE INSERT ON Steps
BEGIN
    UPDATE Steps
    SET Id = Id + 1
    WHERE Id >= NEW.Id;

    UPDATE Buildings
    SET BuildStep = BuildStep + 1
    WHERE BuildStep >= NEW.Id;

    UPDATE Buildings
    SET DestroyStep = DestroyStep + 1
    WHERE DestroyStep >= NEW.Id;
END;
");
            // Trigger to decrement step IDs and adjust building references
            // when a step is deleted
            Database.ExecuteSqlRaw(@"
CREATE TRIGGER IF NOT EXISTS decrement_step_ids_after_delete
AFTER DELETE ON Steps
BEGIN
    UPDATE Steps
    SET Id = Id - 1
    WHERE Id > OLD.Id;

    UPDATE Buildings
    SET BuildStep = BuildStep - 1
    WHERE BuildStep > OLD.Id;

    UPDATE Buildings
    SET DestroyStep = DestroyStep - 1
    WHERE DestroyStep > OLD.Id;
END;
");
        }
    }
}