using System;
using System.Linq;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Build.Configuration;
using Orcus.Administration.Views.Dialogs;
using Orcus.Shared.Compression;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for ConfigurationManagerWindow.xaml
    /// </summary>
    public partial class ConfigurationManagerWindow
    {
        public ConfigurationManagerWindow()
        {
            InitializeComponent();
        }
        /*
        private void ImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            var inputWindow = new InputMultilineTextWindow
            {
                Owner = this,
                Title = (string) Application.Current.Resources["ImportConfiguration"]
            };
            if (inputWindow.ShowDialog() != true)
                return;

            var configurationString = inputWindow.Text.Trim();
            try
            {
                var buildConfiguration = BuildConfiguration.Import(configurationString);
                Settings.Current.BuildConfigurations.Add(buildConfiguration);
                Settings.Current.Save();
                ConfigurationAdded?.Invoke(this, buildConfiguration);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            ConfigurationsListBox.ItemsSource = null;
            ConfigurationsListBox.ItemsSource = Settings.Current.BuildConfigurations;
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedBuildConfiguration = ConfigurationsListBox.SelectedItem as BuildConfiguration;
            if (selectedBuildConfiguration == null)
                return;

            new ExportConfigurationWindow(selectedBuildConfiguration)
            {
                Owner = this
            }.ShowDialog();
        }*/
    }
}