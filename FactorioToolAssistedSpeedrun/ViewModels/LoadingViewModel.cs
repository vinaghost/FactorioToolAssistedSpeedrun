using CommunityToolkit.Mvvm.ComponentModel;

namespace FactorioToolAssistedSpeedrun.ViewModels
{
    public partial class LoadingViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isShown;

        public void Show()
        {
            IsShown = true;
        }

        public void Hide()
        {
            IsShown = false;
        }
    }
}