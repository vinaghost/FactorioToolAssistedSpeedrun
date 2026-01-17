using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetSelectedRowQuery
    {
        public required string ProjectDataFile { get; init; }

        public int Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            var selectedRowSetting = context.Settings
                .FirstOrDefault(s => s.Key == SettingConstants.SelectedRow);
            if (selectedRowSetting is not null &&
                int.TryParse(selectedRowSetting.Value, out var selectedRow))
            {
                return selectedRow;
            }
            else
            {
                context.Settings.Add(new Setting
                {
                    Key = SettingConstants.SelectedRow,
                    Value = "0"
                });
                context.SaveChanges();
            }
            return 0;
        }
    }
}