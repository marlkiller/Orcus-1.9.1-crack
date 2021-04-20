using System;
using System.Text;
using Orcus.Server.Core.UI;

namespace Orcus.Server.CommandLine
{
    public class ConsoleUiImplementation : IUiImplementation
    {
        private const int Bars = 38;
        private readonly object _updateLock = new object();
        private string _currentText = string.Empty;

        public void ShowProgressBar(ProgressBarInfo progressBarInfo)
        {
            Console.Write(progressBarInfo.Message + " ");

            progressBarInfo.ProgressChanged += (sender, d) =>
            {
                if (progressBarInfo.IsClosed)
                    return;

                var progressBlockCount = (int) (d*Bars);
                var percent = (int) (d*100);

                var text = string.Format("[{0}{1}] {2,3}%",
                    progressBlockCount == 0 ? "" : new string('=', progressBlockCount - 1) + ">",
                    new string(' ', Bars - progressBlockCount),
                    percent);
                UpdateText(text);
            };
            progressBarInfo.Closed += (sender, args) =>
            {
                UpdateText(string.Empty);
                Console.WriteLine("Done");
            };
        }

        //Thanks to https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54
        private void UpdateText(string text)
        {
            lock (_updateLock)
            {
                // Get length of common portion
                var commonPrefixLength = 0;
                var commonLength = Math.Min(_currentText.Length, text.Length);
                while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
                {
                    commonPrefixLength++;
                }

                // Backtrack to the first differing character
                var outputBuilder = new StringBuilder();
                outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

                // Output new suffix
                outputBuilder.Append(text.Substring(commonPrefixLength));

                // If the new text is shorter than the old one: delete overlapping characters
                var overlapCount = _currentText.Length - text.Length;
                if (overlapCount > 0)
                {
                    outputBuilder.Append(' ', overlapCount);
                    outputBuilder.Append('\b', overlapCount);
                }

                Console.Write(outputBuilder);
                _currentText = text;
            }
        }
    }
}