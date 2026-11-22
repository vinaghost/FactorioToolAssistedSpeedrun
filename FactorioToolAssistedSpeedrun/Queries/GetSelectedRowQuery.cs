using FactorioToolAssistedSpeedrun.Constants;
using FactorioToolAssistedSpeedrun.Entities;

namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetSelectedRowQuery
    {
        public required string ProjectDataFile { get; init; }

        public int Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            var selectedRowSetting = context.GetSetting(SettingConstants.SelectedRow);
            if (selectedRowSetting is not null &&
                int.TryParse(selectedRowSetting.Value, out var selectedRow))
            {
                return selectedRow;
            }
            else
            {
                context.AddSetting(new Setting
                {
                    Key = SettingConstants.SelectedRow,
                    Value = "0"
                });
            }
            return 0;
        }
    }
}