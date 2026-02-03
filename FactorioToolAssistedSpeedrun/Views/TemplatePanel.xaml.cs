using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for TemplatePanel.xaml
    /// </summary>
    public partial class TemplatePanel : UserControl
    {
        public TemplatePanel()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<TemplatePanelViewModel>();
            Loaded += TemplatePanel_Loaded;
        }

        // Dependency Property for SelectedItems
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems),
                typeof(IList),
                typeof(TemplatePanel),
                new PropertyMetadata(null));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        private void TemplatePanel_Loaded(object sender, RoutedEventArgs e)
        {
            Steps.SelectionChanged += Steps_SelectionChanged;
            // Initialize property with current selection
            SelectedItems = Steps.SelectedItems;
        }

        private void Steps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                // Update the dependency property when selection changes
                SelectedItems = grid.SelectedItems;
            }
        }
    }
}