using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetImportIntoRowQuery
    {
        public required string ProjectDataFile { get; init; }

        public int Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            var importIntoRowSetting = context.GetSetting(SettingConstants.ImportIntoRow);
            if (importIntoRowSetting is not null &&
                int.TryParse(importIntoRowSetting.Value, out var importIntoRow))
            {
                return importIntoRow;
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.ImportIntoRow,
                    Value = "0"
                });
            }
            return 0;
        }
    }
}