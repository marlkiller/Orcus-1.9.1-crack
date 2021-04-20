using System;
using System.Windows.Media.Imaging;
using CSCore;
using CSCore.Codecs.MP3;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;
using Sorzus.Wpf.Toolkit;

namespace AudioPackBuilder.Core
{
    public class AudioFile : PropertyChangedBase
    {
        private readonly BitmapSource _waveFormImage;
        private AudioGenre _audioGenre;
        private string _name;
        private BitmapSource _thumbnail;
        private string _thumbnailPath;

        public AudioFile(string fileName)
        {
            Path = fileName;
            Name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            using (var waveSoure = new DmoMp3Decoder(fileName))
            {
                Duration = waveSoure.GetLength();
                Duration = Duration.Subtract(TimeSpan.FromMilliseconds(Duration.Milliseconds));
                _waveFormImage = AudioWaveFormDrawer.DrawAudio(waveSoure);
            }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public AudioGenre AudioGenre
        {
            get { return _audioGenre; }
            set { SetProperty(value, ref _audioGenre); }
        }

        public TimeSpan Duration { get; }

        public BitmapSource Thumbnail
        {
            get { return _thumbnail ?? _waveFormImage; }
            private set { SetProperty(value, ref _thumbnail); }
        }

        public string Path { get; }

        public string ThumbnailPath
        {
            get { return _thumbnailPath; }
            set
            {
                _thumbnailPath = value;
                Thumbnail = !string.IsNullOrEmpty(_thumbnailPath)
                    ? new BitmapImage(new Uri(value, UriKind.Absolute))
                    : null;
            }
        }
    }
}