using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.UI;
using System.Globalization;
using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Conventers
{
    public class StepToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not StepModel step) throw new ArgumentException("Value must be of type StepModel", nameof(value));

            //if (!string.IsNullOrEmpty(step.Color)) return Color.FromName(step.Color);

            return step.Type switch
            {
                StepType.Build => "Cyan",
                StepType.Stop => "Red",
                StepType.Craft => "LightGray",
                StepType.Speed or StepType.Pause or StepType.Save or StepType.KeepCrafting or StepType.KeepOnPath or StepType.KeepWalking or StepType.NeverIdle => "Yellow",
                _ => "White",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}