using Orcus.Server.Core.UI;

namespace Orcus.Server.Ui
{
    public class WinFormsUiImplementation : IUiImplementation
    {
        private readonly MainForm _mainForm;

        public WinFormsUiImplementation(MainForm mainForm)
        {
            _mainForm = mainForm;
        }

        public void ShowProgressBar(ProgressBarInfo progressBarInfo)
        {
            _mainForm.ShowProgressBar(progressBarInfo.Message);
            progressBarInfo.ProgressChanged += (sender, d) =>
            {
                _mainForm.ChangeProgress(d);
            };
            progressBarInfo.Closed += (sender, args) =>
            {
                _mainForm.HideProgressBar();
            };
        }
    }
}