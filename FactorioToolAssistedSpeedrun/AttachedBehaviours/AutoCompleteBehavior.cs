/// Credit to https://github.com/Nimgoble/WPFTextBoxAutoComplete
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioToolAssistedSpeedrun.AttachedBehaviours
{
    public static class AutoCompleteBehavior
    {
        private static readonly TextChangedEventHandler onTextChanged = new(OnTextChanged);
        private static readonly KeyEventHandler onKeyDown = new(OnPreviewKeyDown);

        /// <summary>
        /// The collection to search for matches from.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSourceProperty =
            DependencyProperty.RegisterAttached
            (
                "AutoCompleteItemsSource",
                typeof(IEnumerable<string>),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
            );

        public static IEnumerable<string> GetAutoCompleteItemsSource(DependencyObject target) =>
            (IEnumerable<string>)target.GetValue(AutoCompleteItemsSourceProperty);

        public static void SetAutoCompleteItemsSource(DependencyObject target, IEnumerable<string> value) =>
            target.SetValue(AutoCompleteItemsSourceProperty, value);

        /// <summary>
        /// Whether or not to ignore case when searching for matches.
        /// </summary>
        public static readonly DependencyProperty AutoCompletestringComparison =
            DependencyProperty.RegisterAttached
            (
                "AutoCompletestringComparison",
                typeof(StringComparison),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(StringComparison.OrdinalIgnoreCase)
            );

        public static StringComparison GetAutoCompletestringComparison(DependencyObject target) =>
            (StringComparison)target.GetValue(AutoCompletestringComparison);

        public static void SetAutoCompletestringComparison(DependencyObject target, StringComparison value) =>
            target.SetValue(AutoCompletestringComparison, value);

        private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            //If we're being removed, remove the callbacks
            //Remove our old handler, regardless of if we have a new one.
            textBox.TextChanged -= onTextChanged;
            textBox.PreviewKeyDown -= onKeyDown;
            if (e.NewValue is not null)
            {
                //New source.  Add the callbacks
                textBox.TextChanged += onTextChanged;
                textBox.PreviewKeyDown += onKeyDown;
            }
        }

        /// <summary>
        /// Used for moving the caret to the end of the suggested auto-completion text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (e.OriginalSource is not TextBox tb)
                return;

            //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
            if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Search for auto-completion suggestions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is null)
                return;

            if (e.OriginalSource is not TextBox textBox)
                return;

            //No reason to search if there's nothing there.
            if (string.IsNullOrEmpty(textBox.Text))
                return;

            // If we're deleting text, don't try to auto-complete.
            if (e.Changes.Sum(x => x.RemovedLength) > 0 && e.Changes.Sum(x => x.AddedLength) == 0)
                return;

            IEnumerable<string> values = GetAutoCompleteItemsSource(textBox);
            //No reason to search if we don't have any values.
            if (values == null || !values.Any())
                return;

            int startIndex = 0; //Start from the beginning of the line.
            var textLength = textBox.Text.Length;

            var comparer = GetAutoCompletestringComparison(textBox);
            //Do search and changes here.

            var match = values
                .Where(x => !string.IsNullOrEmpty(x) && x.Length >= textLength)
                .Where(x => x.StartsWith(textBox.Text, comparer))
                .OrderBy(x => x)
                .FirstOrDefault();

            //Nothing.  Leave 'em alone
            if (string.IsNullOrEmpty(match))
                return;

            textBox.TextChanged -= onTextChanged;

            textBox.Text += match[textLength..];
            int matchStart = (startIndex + textLength);
            textBox.CaretIndex = matchStart;
            textBox.SelectionStart = matchStart;
            textBox.SelectionLength = (match.Length - textLength);

            textBox.TextChanged += onTextChanged;
        }
    }
}