using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Services;
using System.Globalization;
using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Misc.Conventers
{
    public class ItemNameToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataService = Ioc.Default.GetRequiredService<IDataService>();
            if (!dataService.IsGameDataLoaded) throw new InvalidOperationException("GameData is not initialized.");

            if (value is string itemName && !string.IsNullOrEmpty(itemName))
            {
                if (dataService.GameData.ItemsLocale.TryGetValue(itemName, out var humanized))
                    return humanized;
                return itemName;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataService = Ioc.Default.GetRequiredService<IDataService>();
            if (!dataService.IsGameDataLoaded) throw new InvalidOperationException("GameData is not initialized.");

            if (value is string humanizedName && !string.IsNullOrEmpty(humanizedName))
            {
                if (dataService.GameData.ReverseItemsLocale.TryGetValue(humanizedName, out var itemName))
                    return itemName;
                return humanizedName;
            }
            return value;
        }
    }
}