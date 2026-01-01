using FactorioToolAssistedSpeedrun.Enums;
using System.Globalization;
using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Conventers
{
    public class StepTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StepType stepType)
            {
                return StepTypeExtensions.ToString(stepType);
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return StepTypeExtensions.FromString(str);
            }
            throw new InvalidOperationException("Invalid conversion");
        }
    }
}