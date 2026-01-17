using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetImportIntoRowQuery
    {
        public required string ProjectDataFile { get; init; }

        public int Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            var importIntoRowSetting = context.Settings
                .FirstOrDefault(s => s.Key == SettingConstants.ImportIntoRow);
            if (importIntoRowSetting is not null &&
                int.TryParse(importIntoRowSetting.Value, out var importIntoRow))
            {
                return importIntoRow;
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.ImportIntoRow,
                    Value = "0"
                });
                context.SaveChanges();
            }
            return 0;
        }
    }
}