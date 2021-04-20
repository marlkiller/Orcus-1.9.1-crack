using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using MahApps.Metro;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.AppSettings;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class SettingsViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private RelayCommand _closeCommand;
        private RelayCommand _restartCommand;
        private bool _restartRequired;
        private AccentColorViewModel _selectedAccentColor;
        private LanguageInfo _selectedLanguage;
        private ApplicationTheme _theme;
        private RelayCommand _throwTestExceptionCommand;

        public SettingsViewModel(Settings settings, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            Settings = settings;
            _selectedLanguage = settings.Language;
            _theme = settings.Theme;
            RefreshRestartRequired();

            AccentColorMenuDatas =
                new[]
                {
                    "Blue", "Cobalt", "Purple", "Emerald", "Teal", "Brown", "Orange", "Red", "Crimson", "Pink",
                    "Magenta",
                    "Steel"
                }.Select(
                    x => new AccentColorViewModel(x)).ToList();
            _selectedAccentColor = AccentColorMenuDatas.First(x => x.Name == Settings.AccentColor);
        }

        public Settings Settings { get; }
        public List<AccentColorViewModel> AccentColorMenuDatas { get; }

        public LanguageInfo SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (SetProperty(value, ref _selectedLanguage))
                {
                    Settings.Language = value;
                    Settings.Language.Load();
                }
            }
        }

        public bool RestartRequired
        {
            get { return _restartRequired; }
            set { SetProperty(value, ref _restartRequired); }
        }

        public ApplicationTheme Theme
        {
            get { return _theme; }
            set
            {
                if (SetProperty(value, ref _theme))
                {
                    Settings.Theme = value;
                    RefreshRestartRequired();
                }
            }
        }

        public AccentColorViewModel SelectedAccentColor
        {
            get { return _selectedAccentColor; }
            set
            {
                if (SetProperty(value, ref _selectedAccentColor))
                {
                    Settings.AccentColor = value.Name;
                    Settings.LoadAccent(value.Name);
                    Settings.Save();
                    //Invalidates global colors and resources.
                    var method = typeof (ThemeManager).GetMethod("OnThemeChanged",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    method.Invoke(null,
                        new object[]
                        {
                            ThemeManager.Accents.First(x => x.Name == value.Name),
                            ThemeManager.AppThemes.First(
                                x => x.Name == (Settings.Theme == ApplicationTheme.Dark ? "BaseDark" : "BaseLight"))
                        });
                }
            }
        }

        public RelayCommand RestartCommand
        {
            get
            {
                return _restartCommand ?? (_restartCommand = new RelayCommand(parameter =>
                {
                    Settings.Save();
                    ApplicationInterface.ForceShutdown = true;
                    Application.Current.Restart(
                        $"/s {_connectionManager.Ip} /p {_connectionManager.Port} /password {_connectionManager.Password} /a true");
                }));
            }
        }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => { Settings.Save(); })); }
        }

        public RelayCommand ThrowTestExceptionCommand
        {
            get
            {
                return _throwTestExceptionCommand ?? (_throwTestExceptionCommand = new RelayCommand(parameter =>
                {
                    if (WindowServiceInterface.Current.ShowMessageBox("This app will be thrown up by pressing Ok.",
                        "Warning",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        throw new InvalidOperationException("This is a test exception!");
                }));
            }
        }

        private void RefreshRestartRequired()
        {
            RestartRequired = Settings.Theme != Settings.AppliedTheme;
        }
    }
}