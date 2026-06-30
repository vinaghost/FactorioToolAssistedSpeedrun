using CommunityToolkit.Mvvm.DependencyInjection;
using FactorioToolAssistedSpeedrun.Services;
using FactorioToolAssistedSpeedrun.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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
            DataContext = Ioc.Default.GetRequiredService<StepPanelViewModel>();
        }

        public void ScrollToSelected()
        {
            if (_scrollViewer is null) return;

            var rowHeight = _scrollViewer.ScrollableHeight / (Steps.Items.Count - 1);
            var index = Math.Max(0, Steps.SelectedIndex - 100);
            var offset = index * rowHeight;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _scrollViewer.ScrollToVerticalOffset(offset);
                Steps.ScrollIntoView(Steps.SelectedItem);
            }));
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
            var panelService = Ioc.Default.GetRequiredService<PanelService>();
            panelService.ScrollToSelectedStep = ScrollToSelected;
            panelService.StepsChangeStarted = Steps.BeginInit;
            panelService.StepsChangeCompleted = Steps.EndInit;

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