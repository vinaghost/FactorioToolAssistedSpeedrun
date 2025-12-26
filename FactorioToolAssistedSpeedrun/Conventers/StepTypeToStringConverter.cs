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
                return stepType.ToStepTypeString();
            }
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.ToStepType();
            }
            throw new InvalidOperationException("Invalid conversion");
        }
    }
}