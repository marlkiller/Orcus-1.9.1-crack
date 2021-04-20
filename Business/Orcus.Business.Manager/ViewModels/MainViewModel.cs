using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Orcus.Business.Manager.Core;
using Orcus.Business.Manager.Core.Data;
using Orcus.Business.Manager.Utilities;
using Orcus.Business.Manager.Views;
using Sorzus.Wpf.Toolkit;
using License = Orcus.Business.Manager.Core.Data.License;

namespace Orcus.Business.Manager.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private RelayCommand _banLicensesCommand;
        private RelayCommand _changeLicensesCommentCommand;
        private RelayCommand _clearComputersCommand;
        private DatabaseInfo _databaseInfo;
        private RelayCommand _deleteLicensesCommand;
        private RelayCommand _generateLicensesCommand;
        private ObservableCollection<License> _licenses;
        private ICollectionView _licensesCollectionView;
        private RelayCommand _openLicenseInformationCommand;
        private string _searchLicenses;
        private RelayCommand _unbanLicensesCommand;

        public MainViewModel()
        {
            Application.Current.MainWindow.ContentRendered += MainWindow_ContentRendered;
        }

        public ObservableCollection<License> Licenses
        {
            get { return _licenses; }
            set { SetProperty(value, ref _licenses); }
        }

        public ICollectionView LicensesCollectionView
        {
            get { return _licensesCollectionView; }
            set { SetProperty(value, ref _licensesCollectionView); }
        }

        public string SearchLicenses
        {
            get { return _searchLicenses; }
            set
            {
                if (SetProperty(value, ref _searchLicenses))
                    LicensesCollectionView.Refresh();
            }
        }

        public RelayCommand GenerateLicensesCommand
        {
            get
            {
                return _generateLicensesCommand ?? (_generateLicensesCommand = new RelayCommand(parameter =>
                {
                    var window = new GenerateLicensesWindow {Owner = Application.Current.MainWindow};
                    window.ShowDialog();
                    foreach (var license in window.Licenses)
                        Licenses.Add(license);
                    RefreshStatistics();
                }));
            }
        }

        public RelayCommand OpenLicenseInformationCommand
        {
            get
            {
                return _openLicenseInformationCommand ?? (_openLicenseInformationCommand = new RelayCommand(parameter =>
                {
                    var window = new LicenseInfoWindow((License) parameter, _databaseInfo)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    window.ShowDialog();
                    if (window.RefreshLicenses)
                    {
                        LicensesCollectionView.Refresh();
                        RefreshStatistics();
                    }
                }));
            }
        }

        public RelayCommand UnbanLicensesCommand
        {
            get
            {
                return _unbanLicensesCommand ?? (_unbanLicensesCommand = new RelayCommand(async parameter =>
                {
                    var parameters = ((IList) parameter).Cast<License>().ToList();
                    if (await WebServerConnection.ChangeBanValueLicense(parameters, false))
                    {
                        foreach (var license in parameters)
                        {
                            license.banned = "0";
                        }
                        LicensesCollectionView.Refresh();
                    }
                }));
            }
        }

        public RelayCommand BanLicensesCommand
        {
            get
            {
                return _banLicensesCommand ?? (_banLicensesCommand = new RelayCommand(async parameter =>
                {
                    var parameters = ((IList) parameter).Cast<License>().ToList();
                    if (await WebServerConnection.ChangeBanValueLicense(parameters, true))
                    {
                        foreach (var license in parameters)
                        {
                            license.banned = "1";
                        }
                        LicensesCollectionView.Refresh();
                        RefreshStatistics();
                    }
                }));
            }
        }

        public RelayCommand DeleteLicensesCommand
        {
            get
            {
                return _deleteLicensesCommand ?? (_deleteLicensesCommand = new RelayCommand(async parameter =>
                {
                    var parameters = ((IList) parameter).Cast<License>().ToList();

                    if (
                        MessageBoxEx.Show(Application.Current.MainWindow,
                            $"Are you sure that you want to delete {parameters.Count} licenses?", "Errors",
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;

                    if (await WebServerConnection.RemoveLicenses(parameters))
                    {
                        foreach (var license in parameters)
                            Licenses.Remove(license);

                        LicensesCollectionView.Refresh();
                        RefreshStatistics();
                    }
                }));
            }
        }

        public RelayCommand ChangeLicensesCommentCommand
        {
            get
            {
                return _changeLicensesCommentCommand ??
                       (_changeLicensesCommentCommand = new RelayCommand(async parameter =>
                       {
                           var parameters = ((IList) parameter).Cast<License>().ToList();
                           var window = new ChangeCommentWindow {Owner = Application.Current.MainWindow};
                           if (window.ShowDialog() == true)
                           {
                               if (await WebServerConnection.SetLicensesComment(parameters, window.Comment))
                               {
                                   foreach (var license in parameters)
                                       license.comment = window.Comment;

                                   LicensesCollectionView.Refresh();
                               }
                           }
                       }));
            }
        }

        public RelayCommand ClearComputersCommand
        {
            get
            {
                return _clearComputersCommand ?? (_clearComputersCommand = new RelayCommand(async parameter =>
                {
                    var parameters = ((IList) parameter).Cast<License>().ToList();

                    if (
                        MessageBoxEx.Show(Application.Current.MainWindow,
                            $"Are you sure that you want to clear all computers of {parameters.Count} licenses?",
                            "Errors",
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;

                    await WebServerConnection.ClearComputers(parameters);
                }));
            }
        }

        private int _bannedLicensesCount;

        public int BannedLicensesCount
        {
            get { return _bannedLicensesCount; }
            set { SetProperty(value, ref _bannedLicensesCount); }
        }

        private int _paidLicensesCount;

        public int PaidLicensesCount
        {
            get { return _paidLicensesCount; }
            set { SetProperty(value, ref _paidLicensesCount); }
        }

        private void RefreshStatistics()
        {
            BannedLicensesCount = Licenses.Count(x => x.IsBanned);
            PaidLicensesCount = Licenses.Count(x => !x.NoMoney);
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            //unsubscribe
            Application.Current.MainWindow.ContentRendered -= MainWindow_ContentRendered;

            var loginWindow = new LoginWindow {Owner = Application.Current.MainWindow};
            if (loginWindow.ShowDialog() != true)
                Environment.Exit(0);

            WebServerConnection.Token = loginWindow.Password;

            var loadingWindow = new DownloadingDataWindow {Owner = Application.Current.MainWindow};
            if (loadingWindow.ShowDialog() == true)
            {
                _databaseInfo = loadingWindow.DatabaseInfo;
                Licenses = new ObservableCollection<License>(_databaseInfo.licenses);
                LicensesCollectionView = CollectionViewSource.GetDefaultView(Licenses);
                LicensesCollectionView.Filter += FilterLicenses;
                RefreshStatistics();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private bool FilterLicenses(object o)
        {
            if (string.IsNullOrWhiteSpace(SearchLicenses))
                return true;

            var license = (License) o;

            Guid parsedGuid;
            if (Guid.TryParse(SearchLicenses, out parsedGuid))
                return license.licenseKey == parsedGuid.ToString("N");

            if (SearchLicenses.IndexOf("is:paid", StringComparison.OrdinalIgnoreCase) > -1 && license.NoMoney)
                return false;
            if (SearchLicenses.IndexOf("isnot:paid", StringComparison.OrdinalIgnoreCase) > -1 && !license.NoMoney)
                return false;

            if (SearchLicenses.IndexOf("is:banned", StringComparison.OrdinalIgnoreCase) > -1 && !license.IsBanned)
                return false;
            if (SearchLicenses.IndexOf("isnot:banned", StringComparison.OrdinalIgnoreCase) > -1 && license.IsBanned)
                return false;

            var fixedText =
                SearchLicenses.Replace("is:banned", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("isnot:banned", "", StringComparison.OrdinalIgnoreCase).Replace("is:paid", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("isnot:paid", "", StringComparison.OrdinalIgnoreCase);

            return license.licenseKey.IndexOf(fixedText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   license.comment.IndexOf(fixedText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   _databaseInfo.computers.Any(
                       x =>
                           x.licenseId == license.id &&
                           string.Equals(x.hardwareId, fixedText, StringComparison.OrdinalIgnoreCase));
        }
    }
}