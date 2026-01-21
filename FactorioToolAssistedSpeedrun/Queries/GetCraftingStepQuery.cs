using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetCraftingStepQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<Step> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return [.. context.Steps
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(x.Name))
                .Where(x => x.Type == StepType.Craft)
                .OrderBy(x => x.Location)];
        }
    }
}