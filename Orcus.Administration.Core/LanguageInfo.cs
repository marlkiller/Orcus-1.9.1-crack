using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;

namespace Orcus.Administration.Core
{
    public class LanguageInfo
    {
        private static ResourceDictionary _lastLanguageResourceDictionary;

        public LanguageInfo(Uri resourceUri, CultureInfo cultureInfo)
        {
            Uri = resourceUri;
            CultureInfo = cultureInfo;
            Name = cultureInfo.DisplayName;
        }

        public LanguageInfo()
        {
        }

        [JsonIgnore]
        public string Name { get; }

        [JsonIgnore]
        public CultureInfo CultureInfo { get; }

        public Uri Uri { get; set; }
        public string Culture { get; set; }

        public void Load()
        {
            var lastLanguage = _lastLanguageResourceDictionary;
            var file = new FileInfo(Uri.OriginalString);
            _lastLanguageResourceDictionary = new ResourceDictionary
            {
                Source = file.Exists ? new Uri(file.FullName, UriKind.Absolute) : Uri
            };
            Application.Current.Resources.MergedDictionaries.Add(_lastLanguageResourceDictionary);
            if (lastLanguage != null)
                Application.Current.Resources.MergedDictionaries.Remove(lastLanguage);

            Thread.CurrentThread.CurrentCulture = CultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo;
        }
    }
}