using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Misc.Conventers
{
    public class CheckboxCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null) return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return value.Equals(true) ? parameter : null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}