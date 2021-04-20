using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;

namespace AudioPackBuilder.Plugin
{
    internal class AudioFile : IAudioFile
    {
        private readonly string _imageResource;
        private readonly string _resourceName;
        private BitmapImage _thumbnail;

        public AudioFile(string resourceName, TimeSpan duration, AudioGenre genre, string name, string imageResource)
        {
            _resourceName = resourceName;
            _imageResource = imageResource;
            Duration = duration;
            Genre = genre;
            Name = name;
        }

        public void Dispose()
        {
            _thumbnail?.StreamSource.Dispose();
        }

        public byte[] Data
        {
            get
            {
                using (var stream = ReadEmbeddedResource(_resourceName))
                    return stream.ToArray();
            }
        }

        public TimeSpan Duration { get; }
        public AudioGenre Genre { get; }
        public string Name { get; }

        public BitmapImage Thumbnail
        {
            get
            {
                if (_thumbnail == null && !string.IsNullOrEmpty(_imageResource))
                {
                    using (var stream = ReadEmbeddedResource(_imageResource))
                    {
                        stream.Position = 0;
                        _thumbnail = new BitmapImage();
                        _thumbnail.BeginInit();
                        _thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                        _thumbnail.StreamSource = stream;
                        _thumbnail.EndInit();
                        _thumbnail.Freeze();
                    }
                }

                return _thumbnail;
            }
        }

        private static MemoryStream ReadEmbeddedResource(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(file);
            if (stream == null)
                return null;

            using (stream)
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream;
            }
        }
    }
}