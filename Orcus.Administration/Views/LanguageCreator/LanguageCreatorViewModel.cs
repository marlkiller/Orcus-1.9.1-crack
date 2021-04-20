using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.LanguageCreator
{
    public class LanguageCreatorViewModel : PropertyChangedBase
    {
        private RelayCommand _closeCommand;
        private LanguageDocument _currentLanguageDocument;
        private string _filePath;
        private RelayCommand _newDocumentCommand;
        private RelayCommand _openDocumentCommand;
        private RelayCommand _resetValuesCommand;
        private RelayCommand _saveDocumentAsCommand;
        private RelayCommand _saveDocumentCommand;
        private CultureInfo _selectedCultureInfo;

        public LanguageCreatorViewModel()
        {
            CultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures).ToArray();
        }

        public CultureInfo[] CultureInfos { get; }

        public CultureInfo SelectedCultureInfo
        {
            get { return _selectedCultureInfo; }
            set { SetProperty(value, ref _selectedCultureInfo); }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(value, ref _filePath); }
        }

        public LanguageDocument CurrentLanguageDocument
        {
            get { return _currentLanguageDocument; }
            set { SetProperty(value, ref _currentLanguageDocument); }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand ??
                       (_closeCommand = new RelayCommand(parameter => { Application.Current.Shutdown(); }));
            }
        }

        public RelayCommand OpenDocumentCommand
        {
            get
            {
                return _openDocumentCommand ?? (_openDocumentCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        Filter = "XAML files|*.xaml|All files|*.*",
                        Multiselect = false,
                        CheckFileExists = true
                    };

                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        try
                        {
                            CurrentLanguageDocument = LanguageDocument.FromFile(ofd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxEx.Show(Application.Current.MainWindow, ex.Message,
                                (string) Application.Current.Resources["Error"],
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        FilePath = ofd.FileName;
                        var match = Regex.Match(Path.GetFileName(ofd.FileName),
                            @"^OrcusAdministration\.(?<language>([a-z]{2}(-[a-z]{2})?))\.xaml$", RegexOptions.IgnoreCase);
                        if (match.Success)
                            SelectedCultureInfo =
                                CultureInfos.FirstOrDefault(x => x.Name == match.Groups["language"].Value);
                    }
                }));
            }
        }

        public RelayCommand NewDocumentCommand
        {
            get
            {
                return _newDocumentCommand ?? (_newDocumentCommand = new RelayCommand(parameter =>
                {
                    CurrentLanguageDocument = new LanguageDocument();
                    FilePath = null;
                }));
            }
        }

        public RelayCommand SaveDocumentCommand
        {
            get
            {
                return _saveDocumentCommand ?? (_saveDocumentCommand = new RelayCommand(parameter =>
                {
                    if (CurrentLanguageDocument == null) return;
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        SaveAs();
                    }
                    else
                    {
                        CurrentLanguageDocument.SaveDocument(FilePath);
                    }
                }));
            }
        }

        public RelayCommand SaveDocumentAsCommand
        {
            get
            {
                return _saveDocumentAsCommand ?? (_saveDocumentAsCommand = new RelayCommand(parameter =>
                {
                    if (CurrentLanguageDocument != null)
                        SaveAs();
                }));
            }
        }

        public RelayCommand ResetValuesCommand
        {
            get
            {
                return _resetValuesCommand ??
                       (_resetValuesCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   CurrentLanguageDocument?.LanguageEntries.ForEach(x => x.Value = string.Empty);
                               }));
            }
        }

        private void SaveAs()
        {
            if (SelectedCultureInfo == null)
            {
                MessageBoxEx.Show(Application.Current.MainWindow, "No culture selected", "Error", MessageBoxButton.OK);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "XAML files|*.xaml|All files|*.*",
                FileName = $"OrcusAdministration.{SelectedCultureInfo.Name}"
            };

            if (sfd.ShowDialog(Application.Current.MainWindow) == true)
            {
                CurrentLanguageDocument.SaveDocument(sfd.FileName);
                FilePath = sfd.FileName;
                MessageBoxEx.Show(Application.Current.MainWindow, "Document saved", "Success", MessageBoxButton.OK);
            }
        }
    }
}