using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FactorioToolAssistedSpeedrun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<StepModel> BooksCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            BooksCollection = new ObservableCollection<StepModel>
            {
                new StepModel { Step = "Walk", X = 0, Y = 0, Amount = 0, Item = "", Orientation = "N/A", Modifier = "", Comment = "Start position" },
                new StepModel { Step = "Craft", X = 5.5, Y = 7.2, Amount = 1, Item = "Iron Gear", Orientation = "N/A", Modifier = "", Comment = "First craft" },
                new StepModel { Step = "Walk", X = 10, Y = 15, Amount = 0, Item = "", Orientation = "East", Modifier = "Fast", Comment = "Move to next area" },
                new StepModel { Step = "Tech", X = 0, Y = 0, Amount = 0, Item = "Automation", Orientation = "N/A", Modifier = "", Comment = "Research automation" },
                new StepModel { Step = "Craft", X = 12.3, Y = 8.8, Amount = 2, Item = "Transport Belt", Orientation = "N/A", Modifier = "", Comment = "Prepare belts" }
            };
            DataContext = this;
        }
    }

    public class StepModel
    {
        public string Step { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Amount { get; set; }
        public string Item { get; set; }
        public string Orientation { get; set; }
        public string Modifier { get; set; }
        public string Comment { get; set; }
    }
}