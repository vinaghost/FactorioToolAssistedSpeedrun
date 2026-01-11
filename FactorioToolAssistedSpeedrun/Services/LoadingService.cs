using CommunityToolkit.Mvvm.ComponentModel;

namespace FactorioToolAssistedSpeedrun.Services
{
    public partial class LoadingService : ObservableObject
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