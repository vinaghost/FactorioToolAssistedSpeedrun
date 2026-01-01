using System.Globalization;
using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Conventers
{
    public class ItemNameToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (App.Current.GameData is null) throw new InvalidOperationException("GameData is not initialized.");
            if (value is string itemName && !string.IsNullOrEmpty(itemName))
            {
                if (App.Current.GameData.ItemsLocale.TryGetValue(itemName, out var humanized))
                    return humanized;
                return itemName;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (App.Current.GameData is null) throw new InvalidOperationException("GameData is not initialized.");
            if (value is string humanizedName && !string.IsNullOrEmpty(humanizedName))
            {
                if (App.Current.GameData.ReverseItemsLocale.TryGetValue(humanizedName, out var itemName))
                    return itemName;
                return humanizedName;
            }
            return value;
        }
    }
}