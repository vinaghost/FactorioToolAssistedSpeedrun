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
    /// Interaction logic for ImportStringWindow.xaml
    /// </summary>
    public partial class ImportStringWindow : Window
    {
        public ImportStringWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<ImportStringViewModel>();
        }
    }
}