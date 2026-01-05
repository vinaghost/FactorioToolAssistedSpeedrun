using CommunityToolkit.Mvvm.ComponentModel;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class DialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _line;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MinMaxText))]
        private int _minLine;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MinMaxText))]
        private int _maxLine;

        public string MinMaxText => $"({MinLine} - {MaxLine})";
    }
}