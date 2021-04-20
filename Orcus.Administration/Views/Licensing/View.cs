using System;
using Orcus.Administration.Core;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.Licensing
{
    public abstract class View : PropertyChangedBase
    {
        private bool _canGoForward = true;

        public Settings Settings { get; set; }
        public LicenseConfig LicenseConfig { get; set; }

        public bool CanGoForward
        {
            get { return _canGoForward; }
            set
            {
                if (SetProperty(value, ref _canGoForward))
                    GoForwardChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler GoForwardChanged;
    }
}