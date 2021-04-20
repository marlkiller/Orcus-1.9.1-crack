using System.Drawing;
using System.Windows.Forms;

namespace Orcus.Chat.Simple.Utilities
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public static void ScrollToEnd(this RichTextBox box)
        {
            box.SelectionStart = box.Text.Length;
            box.ScrollToCaret();
        }
    }
}