using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for CodeCommandView.xaml
    /// </summary>
    public partial class CodeCommandView
    {
        public CodeCommandView()
        {
            InitializeComponent();

            using (var stringReader = new StringReader(Properties.Resources.BatchSyntax))
            using (var xmlReader = new XmlTextReader(stringReader))
                BatchTextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);

            using (var stringReader = new StringReader(Properties.Resources.CSharpSyntax))
            using (var xmlReader = new XmlTextReader(stringReader))
                CsharpTextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);

            using (var stringReader = new StringReader(Properties.Resources.VisualBasicSyntax))
            using (var xmlReader = new XmlTextReader(stringReader))
                VisualBasicTextEditor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);

            BatchTextEditor.Text = Properties.Resources.BatchSample;

            CsharpTextEditor.TextArea.SetResourceReference(TextArea.SelectionBrushProperty, "AccentColorBrush2");
            VisualBasicTextEditor.TextArea.SetResourceReference(TextArea.SelectionBrushProperty, "AccentColorBrush2");
            BatchTextEditor.TextArea.SetResourceReference(TextArea.SelectionBrushProperty, "AccentColorBrush2");

            CsharpTextEditor.TextArea.SelectionCornerRadius = 0;
            VisualBasicTextEditor.TextArea.SelectionCornerRadius = 0;
            BatchTextEditor.TextArea.SelectionCornerRadius = 0;

            var pen = new Pen((SolidColorBrush) Application.Current.Resources["AccentColorBrush2"], 1);
            CsharpTextEditor.TextArea.SelectionBorder = pen;
            VisualBasicTextEditor.TextArea.SelectionBorder = pen;
            BatchTextEditor.TextArea.SelectionBorder = pen;
        }
    }
}