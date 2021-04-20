using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins.AudioPlugin;

namespace Orcus.Administration.Commands.Audio
{
    public class AudioFile : IAudioFile
    {
        public AudioFile(string name, int id, AudioGenre genre, TimeSpan duration)
        {
            Name = name;
            Id = id;
            Genre = genre;
            Duration = duration;
        }

        public AudioFile(string name, int id, AudioGenre genre, int duration)
            : this(name, id, genre, TimeSpan.FromSeconds(duration))
        {
        }

        public void Dispose()
        {
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public TimeSpan Duration { get; set; }
        public AudioGenre Genre { get; set; }

        public BitmapImage Thumbnail { get; } = null;

        public byte[] Data
        {
            get
            {
                var resource = Application.GetResourceStream(
                    new Uri(
                        $"pack://application:,,,/Orcus.Administration.Resources;component/Audio/{Id}.mp3", UriKind.Absolute));
                using (var ms = new MemoryStream())
                {
                    using (var stream = resource.Stream)
                        stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}