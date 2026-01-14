using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FactorioToolAssistedSpeedrun.Views
{
    /// <summary>
    /// Interaction logic for StepPanel.xaml
    /// </summary>
    public partial class StepPanel : UserControl
    {
        private ScrollViewer? _scrollViewer;

        public StepPanel()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<StepPanelViewModel>();
        }

        public void ScrollToSelected()
        {
            Steps.UpdateLayout();
            _scrollViewer?.ScrollToTop();
            Steps.ScrollIntoView(Steps.SelectedItem);
        }

        public static ScrollViewer? GetScrollViewer(UIElement element)
        {
            if (element is null) return null;

            ScrollViewer? retour = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element) && retour is null; i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is ScrollViewer scrollViewer)
                {
                    retour = scrollViewer;
                }
                else
                {
                    if (VisualTreeHelper.GetChild(element, i) is UIElement child) retour = GetScrollViewer(child);
                }
            }
            return retour;
        }

        private void LoadHandler(object sender, RoutedEventArgs e)
        {
            //vm.StepsChangeStarted = Steps.BeginInit;
            //vm.StepsChangeCompleted = Steps.EndInit;
            var panelService = App.Current.Services.GetRequiredService<PanelService>();
            panelService.ScrollToSelectedStep = ScrollToSelected;

            _scrollViewer = GetScrollViewer(Steps);

            Steps.SelectionChanged += Steps_SelectionChanged;
            SelectedItems = Steps.SelectedItems;
        }

        // Dependency Property for SelectedItems
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems),
                typeof(System.Collections.IList),
                typeof(StepPanel),
                new PropertyMetadata(null));

        public System.Collections.IList SelectedItems
        {
            get => (System.Collections.IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
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