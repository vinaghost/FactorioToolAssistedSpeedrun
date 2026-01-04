using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioToolAssistedSpeedrun.AttachedBehaviours
{
    public static class DataGridRowExtensions
    {
        public static readonly DependencyProperty MouseRightButtonUpCommandProperty =
            DependencyProperty.RegisterAttached(
                "MouseRightButtonUpCommand",
                typeof(ICommand),
                typeof(DataGridRowExtensions),
                new FrameworkPropertyMetadata(default(ICommand), new PropertyChangedCallback(OnSet))
        );

        public static ICommand GetMouseRightButtonUpCommand(DependencyObject target) =>
            (ICommand)target.GetValue(MouseRightButtonUpCommandProperty);

        public static void SetMouseRightButtonUpCommand(DependencyObject target, ICommand value) =>
            target.SetValue(MouseRightButtonUpCommandProperty, value);

        private static void OnSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridRow row)
            {
                row.MouseRightButtonUp += Row_MouseRightButtonUp;
            }
        }

        private static void Row_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                ICommand command = GetMouseRightButtonUpCommand(row);
                command?.Execute(row);
            }
        }
    }
}