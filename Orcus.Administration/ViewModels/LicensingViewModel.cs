using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Licensing;
using Orcus.Administration.Views.Licensing;
using Orcus.Administration.Views.Licensing.Steps;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class LicensingViewModel : PropertyChangedBase
    {
        private readonly LicenseConfig _licenseConfig;
        private readonly List<View> _views;
        private string _animation = "MoveForwardState";
        private bool _canCancel = true;
        private RelayCommand _cancelCommand;
        private bool _canGoBack;
        private bool _canGoForward;
        private View _currentView;
        private CurrentViewMode _currentViewMode;
        private RelayCommand _goBackCommand;
        private RelayCommand _goForwardCommand;

        public LicensingViewModel()
        {
            _licenseConfig = new LicenseConfig();
            _views = new List<View>
            {
                new Step1(Settings.Current, _licenseConfig),
                new Step2(),
                new Step3(Settings.Current, _licenseConfig),
                new Step4(_licenseConfig)
            };

            CurrentView = _views[0];

            _views.ForEach(view => view.GoForwardChanged += (sender, args) => RefreshCanGoForward());
            RefreshCanGoForward();
        }

        public View CurrentView
        {
            get { return _currentView; }
            set { SetProperty(value, ref _currentView); }
        }

        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { SetProperty(value, ref _canGoBack); }
        }

        public bool CanGoForward
        {
            get { return _canGoForward; }
            set { SetProperty(value, ref _canGoForward); }
        }

        public bool CanCancel
        {
            get { return _canCancel; }
            set { SetProperty(value, ref _canCancel); }
        }

        public string Animation
        {
            get { return _animation; }
            set { SetProperty(value, ref _animation); }
        }

        public RelayCommand GoBackCommand
        {
            get
            {
                return _goBackCommand ?? (_goBackCommand = new RelayCommand(parameter =>
                {
                    Animation = "MoveBackwardState";
                    CurrentView = _views[_views.IndexOf(CurrentView) - 1];
                    CanGoBack = _views.IndexOf(CurrentView) > 0;
                    RefreshCanGoForward();
                }));
            }
        }

        public RelayCommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { Environment.Exit(0); })); }
        }

        public RelayCommand GoForwardCommand
        {
            get
            {
                return _goForwardCommand ?? (_goForwardCommand = new RelayCommand(async parameter =>
                {
                    Animation = "MoveForwardState";
                    if (CurrentViewMode == CurrentViewMode.LastStep)
                    {
                        CurrentView = new Step5(_licenseConfig);
                        CanGoBack = false;
                        CanGoForward = false;

                        var result = await WebserverConnection.Current.TryRegister(_licenseConfig.LicenseKey);
                        if (result == LicenseRequestResult.Success)
                        {
                            CurrentViewMode = CurrentViewMode.Finished;
                            CanGoForward = true;
                            CurrentView = new StepFinished();
                        }
                        else
                        {
                            CurrentViewMode = CurrentViewMode.SmallTalk;
                            CanGoForward = true;
                            CanGoBack = false;
                            CurrentView = new StepError(result);
                        }

                        return;
                    }
                    if (CurrentViewMode == CurrentViewMode.Finished)
                    {
                        Settings.Current.Save();
                        if (Settings.Current.Theme != ApplicationTheme.Light)
                        {
                            Application.Current.Restart();
                            return;
                        }
                        Settings.Current.InitializeSettings();
                        EverythingIsAwesome?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    CurrentView = _views[_views.IndexOf(CurrentView) + 1];
                    CanGoBack = _views.IndexOf(CurrentView) != 0;
                    RefreshCanGoForward();
                }));
            }
        }

        public CurrentViewMode CurrentViewMode
        {
            get { return _currentViewMode; }
            set { SetProperty(value, ref _currentViewMode); }
        }

        public event EventHandler EverythingIsAwesome;

        private void RefreshCanGoForward()
        {
            CanGoForward = CurrentView.CanGoForward;
            if (_views.IndexOf(CurrentView) == _views.Count - 1)
                CurrentViewMode = CurrentViewMode.LastStep;
            else
                CurrentViewMode = CurrentViewMode.SmallTalk;
        }
    }

    public enum CurrentViewMode
    {
        SmallTalk,
        LastStep,
        Finished
    }
}