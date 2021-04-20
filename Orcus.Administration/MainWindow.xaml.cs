using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using Orcus.Administration.Core.Args;
using Orcus.Administration.Licensing;
using Orcus.Administration.ViewModels;
using Orcus.Administration.Views.Dialogs;
using Sorzus.Wpf.Toolkit;
using Sorzus.Wpf.Toolkit.Converter;

namespace Orcus.Administration
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _shown;

        public MainWindow()
        {
            InitializeComponent();
            ((MainViewModel) DataContext).Settings.ConsolePositionChanged += Settings_ConsolePositionChanged;
            Settings_ConsolePositionChanged(null, null);
        }

        private RelayCommand _changeSizeCommand;

        public RelayCommand ChangeSizeCommand
        {
            get
            {
                return _changeSizeCommand ?? (_changeSizeCommand = new RelayCommand(parameter =>
                {
                    var window = new ChangeSizeWindow(ActualWidth, ActualHeight) {Owner = this};
                    if (window.ShowDialog() == true)
                    {
                        Width = window.NewWidth;
                        Height = window.NewHeight;
                    }
                }));
            }
        }

        private void Settings_ConsolePositionChanged(object sender, EventArgs e)
        {
            RootGrid.RowDefinitions.Clear();
            if (((MainViewModel) DataContext).Settings.IsConsoleAtTop)
            {
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(150)});
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1)});
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
            }
            else
            {
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1)});
                RootGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(150)});
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (!_shown)
            {
                _shown = true;

                try
                {
                    WebserverConnection.Current.CheckKey().ContinueWith(keyCheck =>
                    {
                        if (keyCheck.Result == KeyCheckResult.Banned || keyCheck.Result == KeyCheckResult.NotFound)
                        {
                            File.Delete("license.orcus");
                            Environment.Exit(0);
                        }
                    });
                }
                catch (Exception)
                {
                    //Doesn't really matter
                    //what actually matters in life?
                }

                var viewModel = (MainViewModel) DataContext;
                if (viewModel.Loaded(true))
                    viewModel.ConnectionManager.PluginUploadStarted += ConnectionManager_PluginUploadStarted;
            }
        }

        private async void ConnectionManager_PluginUploadStarted(object sender, EventArgs e)
        {
            ProgressDialogController progressDialog = null;
            EventHandler<PluginUploadProgressChangedEventArgs> handler = null;
            var alreadyFinished = false;
            handler = (s, args) =>
            {
                progressDialog?.SetProgress(args.Progress);
                progressDialog?.SetMessage(
                    $"{FormatBytesConverter.BytesToString(args.BytesSent)} {Application.Current.Resources["Of"]} {FormatBytesConverter.BytesToString(args.TotalBytes)}");

                if (Math.Abs(args.Progress - 1) < .1)
                {
                    ((MainViewModel) DataContext).ConnectionManager.PluginUploadProgressChanged -= handler;
                    progressDialog?.CloseAsync();
                    alreadyFinished = true;
                }
            };
            ((MainViewModel) DataContext).ConnectionManager.PluginUploadProgressChanged += handler;

            progressDialog = await this.ShowProgressAsync((string)Application.Current.Resources["UploadingPlugin"], "");
            if (alreadyFinished)
                await progressDialog.CloseAsync();
        }
    }
}