using FactorioToolAssistedSpeedrun.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetBuildingsQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<Building> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return [.. context.Buildings
                .AsNoTracking()];
        }
    }
}