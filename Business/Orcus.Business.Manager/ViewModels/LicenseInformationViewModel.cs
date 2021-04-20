using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Business.Manager.Core;
using Orcus.Business.Manager.Core.Data;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Business.Manager.ViewModels
{
    public class LicenseInformationViewModel : PropertyChangedBase
    {
        private RelayCommand _changeDescriptionCommand;
        private bool _isChangingComment;

        public LicenseInformationViewModel(License license, DatabaseInfo databaseInfo)
        {
            License = license;
            RegisteredComputers = databaseInfo.computers.Where(x => x.licenseId == license.id).ToList();
        }

        public List<Computer> RegisteredComputers { get; }
        public License License { get; }

        public RelayCommand ChangeDescriptionCommand
        {
            get
            {
                return _changeDescriptionCommand ?? (_changeDescriptionCommand = new RelayCommand(async parameter =>
                {
                    if (_isChangingComment)
                        return;

                    _isChangingComment = true;
                    var comment = (string) parameter ?? "";
                    if (await WebServerConnection.SetLicensesComment(new List<License> {License}, comment))
                    {
                        MessageBox.Show("The comment of the license was successfully changed.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        License.comment = comment;
                        RefreshLicenses?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show("The comment of the license could not be changed", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    _isChangingComment = false;
                }));
            }
        }

        public event EventHandler RefreshLicenses;
    }
}