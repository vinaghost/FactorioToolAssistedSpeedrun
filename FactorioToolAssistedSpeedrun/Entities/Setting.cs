using Microsoft.EntityFrameworkCore;

namespace FactorioToolAssistedSpeedrun.Entities
{
    [PrimaryKey("Key")]
    public class Setting
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}