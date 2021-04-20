using System;
using System.Windows;
using System.Windows.Media;

namespace Orcus.Administration.ViewModels.AppSettings
{
    public class AccentColorViewModel
    {
        public AccentColorViewModel(string name)
        {
            Name = name;
            DisplayName = (string) Application.Current.Resources[Name];
            var resourceDictionary = new ResourceDictionary
            {
                Source =
                    new Uri($"/Resources/Themes/Accents/{name}.xaml",
                        UriKind.Relative)
            };
            ColorBrush = (Brush) resourceDictionary["AccentColorBrush"];
        }

        public string Name { get; }
        public string DisplayName { get; }
        public Brush ColorBrush { get; }
    }
}