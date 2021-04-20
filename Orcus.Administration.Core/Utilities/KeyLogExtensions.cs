using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Shared.Commands.Keylogger;

namespace Orcus.Administration.Core.Utilities
{
    public static class KeyLogExtensions
    {
        public static string GenerateHtmlText(List<KeyLogEntry> keyLogEntries, bool hideReleaseKeyState,
            bool hideEmptyWindows)
        {
            if (keyLogEntries == null)
                return null;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(@"<html>
  <head>
    <title>Orcus key log</title>
    <meta charset=""utf-8""/>
  </head > ");

            var specialKeyColor = "#FFE67E22";
            var paragraphTagOpened = false;
            var placeholderText = (string) Application.Current.Resources["NoKeyInputs"];

            for (int i = 0; i < keyLogEntries.Count; i++)
            {
                var keyLogEntry = keyLogEntries[i];
                var normalTextEntry = keyLogEntry as NormalText;
                if (normalTextEntry != null)
                {
                    if (!string.IsNullOrEmpty(normalTextEntry.Text))
                        stringBuilder.Append(normalTextEntry.Text);
                    continue;
                }

                var specialKey = keyLogEntry as SpecialKey;
                if (specialKey != null)
                {
                    string text;
                    switch (specialKey.KeyType)
                    {
                        case SpecialKeyType.Shift:
                            text = "Shift";
                            break;
                        case SpecialKeyType.Win:
                            text = "Win";
                            break;
                        case SpecialKeyType.Tab:
                            text = "Tab";
                            break;
                        case SpecialKeyType.Captial:
                            text = "Caps";
                            break;
                        case SpecialKeyType.Return:
                            text = "Enter";
                            break;
                        case SpecialKeyType.Back:
                            text = "<-";
                            break;
                        default:
                            continue;
                    }

                    if (hideReleaseKeyState && !specialKey.IsDown)
                        continue;

                    stringBuilder.Append(
                        $@"<font color=""{specialKeyColor}"">{{{
                            (hideReleaseKeyState ? "" : (specialKey.IsDown ? "+" : "-"))}{text}}}</font>");
                    continue;
                }

                var standardKey = keyLogEntry as StandardKey;
                if (standardKey != null)
                {
                    if (hideReleaseKeyState && !standardKey.IsDown)
                        continue;

                    string text;
                    switch (standardKey.Key)
                    {
                        case Keys.Alt:
                            text = "Alt";
                            break;
                        case Keys.RMenu:
                            text = "Alt Gr";
                            break;
                        case Keys.Delete:
                            text = "Del";
                            break;
                        case Keys.Control:
                        case Keys.LControlKey:
                            text = "Ctrl";
                            break;
                        default:
                            text = standardKey.ToString();
                            break;
                    }

                    stringBuilder.Append(
                        $@"<font color=""{specialKeyColor}"">{{{
                            (hideReleaseKeyState ? "" : (standardKey.IsDown ? "+" : "-"))}{text}}}</font>");
                    continue;
                }

                var windowChanged = keyLogEntry as WindowChanged;
                if (windowChanged != null)
                {
                    if (hideEmptyWindows && keyLogEntries.Count - 1 > i && keyLogEntries[i + 1] is WindowChanged)
                        //if next is window change again
                        continue;

                    if (!hideEmptyWindows && i > 0 && keyLogEntries[i - 1] is WindowChanged)
                        stringBuilder.Append($"<i>({placeholderText})</i>");

                    if (paragraphTagOpened)
                        stringBuilder.Append("</p>");

                    stringBuilder.AppendLine($"<p><b>[{windowChanged.Timestamp}] {windowChanged.WindowTitle}</b><br />");
                    paragraphTagOpened = true;
                }
            }

            if (paragraphTagOpened)
                stringBuilder.Append("</p>\r\n");
            stringBuilder.AppendLine(@"  </body>
</html>");
            return stringBuilder.ToString();
        }
    }
}