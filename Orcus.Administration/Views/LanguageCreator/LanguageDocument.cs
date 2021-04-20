using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Orcus.Administration.Views.LanguageCreator
{
    public class LanguageDocument
    {
        private static List<string> _allKeys;

        public LanguageDocument()
        {
            LanguageEntries = GetEmptyList();
            var germanDictionary = new ResourceDictionary
            {
                Source = new Uri("/Resources/Languages/OrcusAdministration.de-de.xaml", UriKind.Relative)
            };
            var englishDictionary = new ResourceDictionary
            {
                Source = new Uri("/Resources/Languages/OrcusAdministration.en-us.xaml", UriKind.Relative)
            };

            foreach (var languageEntry in LanguageEntries)
            {
                languageEntry.GermanWord = (string) germanDictionary[languageEntry.Key];
                languageEntry.EnglishWord = (string) englishDictionary[languageEntry.Key];
            }
        }

        public List<LanguageEntry> LanguageEntries { get; }

        public static List<string> AllKeys
        {
            get
            {
                if (_allKeys == null)
                {
                    var dictionary = new ResourceDictionary
                    {
                        Source = new Uri("/Resources/Languages/OrcusAdministration.en-us.xaml", UriKind.Relative)
                    };
                    _allKeys = dictionary.Keys.Cast<string>().ToList();
                }
                return _allKeys;
            }
        }

        public void SaveDocument(string path)
        {
            var dictionary = new ResourceDictionary();
            using (var sw = new StreamWriter(path))
            {
                foreach (var languageEntry in LanguageEntries.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                    dictionary.Add(languageEntry.Key, languageEntry.Value);

                XamlWriter.Save(dictionary, sw);
            }
        }

        public static LanguageDocument FromFile(string path)
        {
            return FromDictionary(new ResourceDictionary {Source = new Uri(path)});
        }

        public static LanguageDocument FromDictionary(ResourceDictionary dictionary)
        {
            var document = new LanguageDocument();

            foreach (var key in dictionary.Keys)
            {
                var resourceEntry = document.LanguageEntries.FirstOrDefault(x => x.Key == (string) key);
                if (resourceEntry != null)
                    resourceEntry.Value = (string) dictionary[key];
            }

            return document;
        }

        private static List<LanguageEntry> GetEmptyList()
        {
            return AllKeys.Select(x => new LanguageEntry {Key = x}).ToList();
        }
    }
}