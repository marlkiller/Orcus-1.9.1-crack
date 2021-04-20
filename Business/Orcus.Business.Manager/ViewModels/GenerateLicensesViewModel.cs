using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orcus.Business.Manager.Core;
using Orcus.Business.Manager.Core.Data;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Business.Manager.ViewModels
{
    public class GenerateLicensesViewModel : PropertyChangedBase
    {
        private string _generatedLicenses;
        private RelayCommand _generateLicensesCommand;

        private bool _isLoading;

        public RelayCommand GenerateLicensesCommand
        {
            get
            {
                return _generateLicensesCommand ?? (_generateLicensesCommand = new RelayCommand(async parameter =>
                {
                    var parameters = (object[]) parameter;
                    var amount = (int) (double) parameters[0];
                    var comment = (string) parameters[1];

                    IsLoading = true;
                    var licenses = await WebServerConnection.GenerateLicenses(amount, comment);
                    GeneratedLicenses =
                        licenses.Aggregate(new StringBuilder(),
                            (builder, license) => builder.AppendLine(license.licenseKey)).ToString();
                    LicensesAdded?.Invoke(this, licenses);
                    IsLoading = false;
                }));
            }
        }

        public string GeneratedLicenses
        {
            get { return _generatedLicenses; }
            set { SetProperty(value, ref _generatedLicenses); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public event EventHandler<List<License>> LicensesAdded;
    }
}