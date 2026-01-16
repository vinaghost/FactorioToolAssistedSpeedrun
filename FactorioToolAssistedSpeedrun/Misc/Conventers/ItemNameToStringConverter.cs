using FactorioToolAssistedSpeedrun.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows.Data;

namespace FactorioToolAssistedSpeedrun.Misc.Conventers
{
    public class ItemNameToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var startupService = App.Current.Services.GetRequiredService<StartupService>();
            if (!startupService.IsGameDataLoaded) throw new InvalidOperationException("GameData is not initialized.");

            if (value is string itemName && !string.IsNullOrEmpty(itemName))
            {
                if (startupService.GameData!.ItemsLocale.TryGetValue(itemName, out var humanized))
                    return humanized;
                return itemName;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var startupService = App.Current.Services.GetRequiredService<StartupService>();
            if (!startupService.IsGameDataLoaded) throw new InvalidOperationException("GameData is not initialized.");

            if (value is string humanizedName && !string.IsNullOrEmpty(humanizedName))
            {
                if (startupService.GameData!.ReverseItemsLocale.TryGetValue(humanizedName, out var itemName))
                    return itemName;
                return humanizedName;
            }
            return value;
        }
    }
}