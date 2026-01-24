using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioToolAssistedSpeedrun.Misc.AttachedBehaviours
{
    public static class DeleteRowBehavior
    {
        public static readonly DependencyProperty DeleteRowCommandProperty =
           DependencyProperty.RegisterAttached(
               "DeleteRowCommand",
               typeof(ICommand),
               typeof(DeleteRowBehavior),
               new FrameworkPropertyMetadata(default(ICommand), new PropertyChangedCallback(OnSet)));

        public static ICommand GetDeleteRowCommand(DependencyObject target) =>
            (ICommand)target.GetValue(DeleteRowCommandProperty);

        public static void SetDeleteRowCommand(DependencyObject target, ICommand value) =>
            target.SetValue(DeleteRowCommandProperty, value);

        private static void OnSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGridRow row) return;

            row.PreviewKeyUp += Row_PreviewKeyUp;
        }

        private static void Row_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is not DataGridRow row) return;
            if (e.Key != Key.Delete) return;
            if (row.IsEditing) return;
            if (ItemsControl.ItemsControlFromItemContainer(row) is not DataGrid grid) return;

            e.Handled = true;
            ICommand command = GetDeleteRowCommand(row);
            command?.Execute(grid.SelectedItems);
        }
    }
}