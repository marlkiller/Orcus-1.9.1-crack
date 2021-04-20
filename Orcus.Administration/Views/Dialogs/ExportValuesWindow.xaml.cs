using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ExportValuesWindow.xaml
    /// </summary>
    public partial class ExportValuesWindow
    {
        public ExportValuesWindow(List<string> variables, string defaultText)
        {
            InitializeComponent();

            using (
                var stringReader =
                    new StringReader(Properties.Resources.ExportValueSyntax.Replace("[KEYWORDS]",
                        variables.Aggregate(new StringBuilder(), (x, y) => x.AppendLine($"<Word>{y}</Word>")).ToString()))
                )
            using (var xmlReader = new XmlTextReader(stringReader))
                TextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
            TextEditor.Text = defaultText;
            ValueListBox.ItemsSource = variables;

            TextEditor.TextArea.SetResourceReference(TextArea.SelectionBrushProperty, "AccentColorBrush2");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = new Pen();
        }

        public string ValueFormat { get; private set; }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            ValueFormat = TextEditor.Text;
            DialogResult = true;
        }
    }
}