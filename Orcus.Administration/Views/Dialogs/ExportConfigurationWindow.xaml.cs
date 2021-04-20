using System;
using System.Windows;
using Orcus.Administration.Core.Build.Configuration;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ExportConfigurationWindow.xaml
    /// </summary>
    public partial class ExportConfigurationWindow
    {
        private readonly BuildConfiguration _buildConfiguration;
        private readonly string _defaultText;
        private string _formattedText;

        public ExportConfigurationWindow(BuildConfiguration buildConfiguration)
        {
            _buildConfiguration = buildConfiguration;
            InitializeComponent();
            TextBox.Text = _defaultText = buildConfiguration.Export(false);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void FormatToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_formattedText == null)
                _formattedText = _buildConfiguration.Export(true);
            TextBox.Text = _formattedText;
        }

        private void FormatToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            TextBox.Text = _defaultText;
        }
    }
}