using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.Plugins.Web;
using Orcus.Plugins;

namespace Orcus.Administration.ViewModels.Plugins
{
    public class PublicPluginViewModel : IPluginViewModel, INotifyPropertyChanged, ICloneable
    {
        private readonly object _thumbnailDownloadLock = new object();
        private readonly string _thumbnailUrl;
        private bool _isDownloadingThumbnail;
        private bool _isInstalled;
        private bool _isUpdateAvailable;
        private BitmapImage _thumbnail;

        public PublicPluginViewModel(PublicPluginData pluginData)
        {
            _thumbnailUrl = pluginData.ThumbnailUrl;
            ProjectWebsite = pluginData.ProjectUrl;
            Name = pluginData.Name;
            Version = PluginVersion.Parse(pluginData.Version);
            Description = pluginData.Description;
            Author = pluginData.Author;
            AuthorUrl = pluginData.AuthorUrl;
            Guid = Guid.Parse(pluginData.Guid);
            Type = pluginData.PluginType;
            Tags = pluginData.Tags;
            Downloads = pluginData.DownloadCount;
        }

        public PublicPluginViewModel(string name, PluginVersion version, string description, string author, string authorUrl,
            Guid guid, PluginType type, string tags, string thumbnailUrl, string projectUrl, bool isInstalled,
            bool isUpdateAvailable, string localPath, int downloadCount)
        {
            Name = name;
            Version = version;
            Description = description;
            Author = author;
            AuthorUrl = authorUrl;
            Guid = guid;
            Type = type;
            Tags = tags;
            _thumbnailUrl = thumbnailUrl;
            ProjectWebsite = projectUrl;
            IsInstalled = isInstalled;
            IsUpdateAvailable = isUpdateAvailable;
            LocalPath = localPath;
            Downloads = downloadCount;
        }

        public string Tags { get; }
        public int Downloads { get; }

        public object Clone()
        {
            return new PublicPluginViewModel(Name, Version, Description, Author, AuthorUrl, Guid, Type, Tags, _thumbnailUrl,
                ProjectWebsite, IsInstalled, IsUpdateAvailable, LocalPath, Downloads);
        }

        public string Name { get; set; }
        public PluginVersion Version { get; set; }
        public string Description { get; set; }
        public string Author { get; }
        public string AuthorUrl { get; }
        public Guid Guid { get; }
        public PluginType Type { get; }
        public string LocalPath { get; set; }

        public BitmapImage Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                    DownloadThumbnail();

                return _thumbnail;
            }
        }

        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set
            {
                if (_isUpdateAvailable != value)
                {
                    _isUpdateAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInstalled
        {
            get { return _isInstalled; }
            set
            {
                if (_isInstalled != value)
                {
                    _isInstalled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProjectWebsite { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private async void DownloadThumbnail()
        {
            lock (_thumbnailDownloadLock)
            {
                if (_isDownloadingThumbnail)
                    return;

                _isDownloadingThumbnail = true;
            }

            using (var wc = new WebClient())
            {
                byte[] data;
                try
                {
                    data = await wc.DownloadDataTaskAsync(_thumbnailUrl);
                }
                catch (Exception)
                {
                    return;
                }

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                using (var ms = new MemoryStream(data))
                {
                    image.StreamSource = ms;
                    image.EndInit();
                }

                _thumbnail = image;
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}