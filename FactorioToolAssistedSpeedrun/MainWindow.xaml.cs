using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;
using FactorioToolAssistedSpeedrun.Models;

namespace FactorioToolAssistedSpeedrun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}