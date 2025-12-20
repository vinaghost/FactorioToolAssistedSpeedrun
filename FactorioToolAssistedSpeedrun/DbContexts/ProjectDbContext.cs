using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.DbContexts
{
    public class ProjectDbContext(string name) : DbContext
    {
        public DbSet<Step> Steps { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite($"Data Source={name}");
        }
    }
}