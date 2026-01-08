using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioToolAssistedSpeedrun.AttachedBehaviours
{
    public static class DeleteRowBehavior
    {
        public static readonly DependencyProperty DeleteRowCommandProperty =
           DependencyProperty.RegisterAttached(
               "DeleteRowCommand",
               typeof(ICommand),
               typeof(DeleteRowBehavior),
               new FrameworkPropertyMetadata(default(ICommand), new PropertyChangedCallback(OnSet))

       ); public static ICommand GetDeleteRowCommand(DependencyObject target) =>

            (ICommand)target.GetValue(DeleteRowCommandProperty);

        public static void SetDeleteRowCommand(DependencyObject target, ICommand value) =>
            target.SetValue(DeleteRowCommandProperty, value);

        private static void OnSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid) return;

            grid.PreviewKeyUp += Grid_PreviewKeyUpp;
        }

        private static void Grid_PreviewKeyUpp(object sender, KeyEventArgs e)
        {
            if (sender is not DataGrid grid) return;
            if (e.Key != Key.Delete) return;
            if (grid.SelectedItems is null || grid.SelectedItems.Count == 0) return;

            e.Handled = true;
            ICommand command = GetDeleteRowCommand(grid);
            command?.Execute(grid.SelectedItems);
        }
    }
}