using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit.Extensions;

namespace Orcus.Administration.Core
{
    public class Settings
    {
        private static Settings _current;
        private bool _isConsoleAtTop;
        private ResourceDictionary _lastThemeResourceDictionary;
        private ResourceDictionary _lastAccentResourceDictionary;
        private string _path;

        public Settings()
        {
            Languages = new List<LanguageInfo>
            {
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.en-us.xaml", UriKind.Relative),
                    new CultureInfo("en")),
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.de-de.xaml", UriKind.Relative),
                    new CultureInfo("de")),
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.fi-fi.xaml", UriKind.Relative),
                    new CultureInfo("fi")),
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.fr-fr.xaml", UriKind.Relative),
                    new CultureInfo("fr")),
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.nb-no.xaml", UriKind.Relative),
                    new CultureInfo("nb")),
                new LanguageInfo(new Uri("/Resources/Languages/OrcusAdministration.it-it.xaml", UriKind.Relative),
                    new CultureInfo("it"))
            };

            var languageDirectory = new DirectoryInfo("Languages");
            if (languageDirectory.Exists)
            {
                foreach (var fileInfo in languageDirectory.GetFiles("*.xaml"))
                {
                    var match = Regex.Match(fileInfo.Name,
                        @"^OrcusAdministration\.(?<language>([a-z]{2}(-[a-z]{2})?))\.xaml$", RegexOptions.IgnoreCase);
                    if (match.Success)
                        Languages.Add(
                            new LanguageInfo(
                                new Uri(AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute).MakeRelativeUri(
                                    new Uri(fileInfo.FullName)), new CultureInfo(match.Groups["language"].Value)));
                }
            }
        }

        public static Settings Current => _current ?? (_current = new Settings());

        public string LastServerIp { get; set; }
        public int LastServerPort { get; set; }
        public LanguageInfo Language { get; set; }
        public ApplicationTheme Theme { get; set; }
        public GroupByProperty DefaultListGroupBy { get; set; }
        public ColumnData DefaultListColumnData { get; set; }
        public string AccentColor { get; set; }
        public List<Guid> EnabledPlugins { get; set; }
        public bool LoadCommandViewDataAutomatically { get; set; } = true;

	    public bool UseProxyToConnectToServer { get; set; }
	    public ProxyType ProxyType { get; set; } = ProxyType.Socks5;
	    public string ProxyIpAddress { get; set; }
	    public int ProxyPort { get; set; } = 1080;
	    public bool ProxyAuthenticate { get; set; }
	    public string ProxyUsername { get; set; }
	    public string ProxyPassword { get; set; }

        [JsonIgnore]
        public ApplicationTheme AppliedTheme { get; set; }

        [JsonIgnore]
        public bool AppliedExperimentalEnableIcons { get; set; }

        [JsonIgnore]
        public bool IsLoaded { get; private set; }

        public bool IsConsoleAtTop
        {
            get { return _isConsoleAtTop; }
            set
            {
                if (_isConsoleAtTop != value)
                {
                    _isConsoleAtTop = value;
                    ConsolePositionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [JsonIgnore]
        public List<LanguageInfo> Languages { get; }

        public event EventHandler ConsolePositionChanged;

        public static void Load(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists)
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file.FullName));
                if (string.IsNullOrEmpty(settings.AccentColor))
                    settings.AccentColor = "Blue";

                settings._path = path;
                settings.InitializeSettings();
                _current = settings;
            }
            else
            {
                Current.LastServerIp = "127.0.0.1";
                Current.LastServerPort = 10134;
                Current.AccentColor = "Blue";
                Current._path = file.FullName;
                Current.InitializeSettings();
            }

            if (Current.DefaultListColumnData == null || !Current.DefaultListColumnData.Order.Contains("country")) //in case of an update
                Current.DefaultListColumnData = new ColumnData
                {
                    Order = {"online", "username", "ipaddress", "id", "ostype", "version", "activewindow", "administrator", "service", "country", "language" },
                    Visible = {"online", "username", "ipaddress", "id", "ostype", "country"}
                };

            if (Current.EnabledPlugins == null)
                Current.EnabledPlugins = new List<Guid>();
        }

        private LanguageInfo GetCultureLanguage()
        {
            return
                Languages.FirstOrDefault(
                    x => x.CultureInfo.TwoLetterISOLanguageName == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ??
                Languages.First();
        }

        public void InitializeSettings()
        {
            Language = Languages.FirstOrDefault(x => x.Uri == Language?.Uri) ?? GetCultureLanguage();
            Language.Load();
            LoadTheme(Theme);
            LoadAccent(AccentColor);

            AppliedTheme = Theme;

            IsLoaded = true;
        }

        private void LoadTheme(ApplicationTheme theme)
        {
            var lastThemeResourceDictionary = _lastThemeResourceDictionary;
            _lastThemeResourceDictionary = new ResourceDictionary
            {
                Source =
                    new Uri($"/Resources/Themes/{(theme == ApplicationTheme.Light ? "Light" : "Dark")}.xaml",
                        UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(_lastThemeResourceDictionary);
            if (lastThemeResourceDictionary != null)
                Application.Current.Resources.MergedDictionaries.Remove(lastThemeResourceDictionary);
        }

        public void LoadAccent(string accentColor)
        {
            var lastAccentResourceDictionary = _lastAccentResourceDictionary;
            _lastAccentResourceDictionary = new ResourceDictionary
            {
                Source =
                    new Uri($"/Resources/Themes/Accents/{accentColor}.xaml",
                        UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(_lastAccentResourceDictionary);
            if (lastAccentResourceDictionary != null)
                Application.Current.Resources.MergedDictionaries.Remove(lastAccentResourceDictionary);
        }

        public void Save()
        {
            //We write it first into another file if settings.json already exists so, when the application crashes it doesn't write an empty file
            var file = new FileInfo(FileExtensions.MakeUnique(_path));
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
            file.Refresh();

            if (file.Length == 0)
            {
                file.Delete();
                return;
            }

            if (file.FullName != _path)
            {
                if (File.Exists(_path))
                    File.Delete(_path);
                file.MoveTo(_path);
            }
        }
    }

    public enum ApplicationTheme
    {
        Light,
        Dark
    }

    public enum GroupByProperty
    {
        Group,
        OsType,
        LanguageName,
        GeoLocationCountry,
        None
    }

	public enum ProxyType
	{
		Socks4,
		Socks4a,
		Socks5
	}
}