using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for CraftingWindow.xaml
    /// </summary>
    public partial class CraftingWindow : Window
    {
        public CraftingWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<CraftingViewModel>();
        }
    }
}