using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

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