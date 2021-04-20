using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.KeyLog;
using Orcus.Shared.Commands.Keylogger;
using Sorzus.Wpf.Toolkit;
using StandardKey = Orcus.Shared.Commands.Keylogger.StandardKey;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for KeyLogControl.xaml
    /// </summary>
    public partial class KeyLogControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty KeyLogEntriesProperty = DependencyProperty.Register(
            "KeyLogEntries", typeof (List<KeyLogEntry>), typeof (KeyLogControl),
            new PropertyMetadata(default(List<KeyLogEntry>), PropertyChangedCallback));

        private bool _hideEmptyWindows;
        private bool _hideReleaseKeyState;

        private ObservableCollection<KeyItem> _keyLogContent;
        private RelayCommand _openInBrowserCommand;
        private RelayCommand _openInEditorCommand;
        private RelayCommand _saveCommand;

        public KeyLogControl()
        {
            InitializeComponent();
        }

        public List<KeyLogEntry> KeyLogEntries
        {
            get { return (List<KeyLogEntry>) GetValue(KeyLogEntriesProperty); }
            set { SetValue(KeyLogEntriesProperty, value); }
        }

        public ObservableCollection<KeyItem> KeyLogContent
        {
            get { return _keyLogContent; }
            set
            {
                if (value != _keyLogContent)
                {
                    _keyLogContent = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HideReleaseKeyState
        {
            get { return _hideReleaseKeyState; }
            set
            {
                if (value != _hideReleaseKeyState)
                {
                    _hideReleaseKeyState = value;
                    UpdateView(KeyLogEntries);
                }
            }
        }

        public bool HideEmptyWindows
        {
            get { return _hideEmptyWindows; }
            set
            {
                if (value != _hideEmptyWindows)
                {
                    _hideEmptyWindows = value;
                    UpdateView(KeyLogEntries);
                }
            }
        }

        public RelayCommand OpenInBrowserCommand
        {
            get
            {
                return _openInBrowserCommand ?? (_openInBrowserCommand = new RelayCommand(parameter =>
                {
                    var file =
                        new FileInfo(Path.Combine(Path.GetTempPath(), "2358A00B-DA82-49AD-ADBD-9DEA191833DC.html"));
                    File.WriteAllText(file.FullName,
                        KeyLogExtensions.GenerateHtmlText(KeyLogEntries, HideReleaseKeyState, HideEmptyWindows));
                    Process.Start(file.FullName);
                }));
            }
        }

        public RelayCommand OpenInEditorCommand
        {
            get
            {
                return _openInEditorCommand ??
                       (_openInEditorCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   NotepadHelper.ShowMessage(GenerateRawText(KeyLogEntries), "Orcus - Key log");
                               }));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog {Filter = "HTML|*.html|Text|*.txt", AddExtension = true};
                    if (sfd.ShowDialog() == true)
                    {
                        string text;
                        switch (sfd.FilterIndex)
                        {
                            case 1:
                                text = KeyLogExtensions.GenerateHtmlText(KeyLogEntries, HideReleaseKeyState,
                                    HideEmptyWindows);
                                break;
                            case 2:
                                text = GenerateRawText(KeyLogEntries);
                                break;
                            default:
                                return;
                        }

                        File.WriteAllText(sfd.FileName, text);
                    }
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var list = dependencyPropertyChangedEventArgs.NewValue as List<KeyLogEntry>;
            var keyLogControl = dependencyObject as KeyLogControl;
            keyLogControl?.UpdateView(list);
        }

        private void UpdateView(List<KeyLogEntry> keyLogEntries)
        {
            var newContent = new List<KeyItem>();

            if (keyLogEntries == null)
            {
                KeyLogContent = new ObservableCollection<KeyItem>();
                OnPropertyChanged(nameof(KeyLogContent));
                return;
            }

            var currentKeyItem = new KeyItem();
            var placeholderText = Application.Current.Resources["NoKeyInputs"];

            foreach (var keyLogEntry in keyLogEntries)
            {
                var normalTextEntry = keyLogEntry as NormalText;
                if (normalTextEntry != null)
                {
                    if (!string.IsNullOrEmpty(normalTextEntry.Text))
                        currentKeyItem.InlineCollection.Add(normalTextEntry.Text);
                    continue;
                }

                var specialKey = keyLogEntry as SpecialKey;
                if (specialKey != null)
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
                            continue;
                    }

                    if (HideReleaseKeyState && !specialKey.IsDown)
                        continue;

                    key.IsPressed = specialKey.IsDown;
                    currentKeyItem.InlineCollection.Add(key);
                    continue;
                }

                var standardKey = keyLogEntry as StandardKey;
                if (standardKey != null)
                {
                    if (HideReleaseKeyState && !standardKey.IsDown)
                        continue;

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
                    currentKeyItem.InlineCollection.Add(new ViewModels.KeyLog.StandardKey(text)
                    {
                        IsPressed = standardKey.IsDown
                    });
                    continue;
                }

                var windowChanged = keyLogEntry as WindowChanged;
                if (windowChanged != null)
                {
                    if (currentKeyItem.InlineCollection.Count == 0)
                    {
                        if (HideEmptyWindows)
                            newContent.Remove(currentKeyItem);
                        else
                            currentKeyItem.InlineCollection.Add(
                                new Italic(new Run("(" + placeholderText + ")")
                                {
                                    Foreground = (Brush) Application.Current.Resources["BlackBrush"]
                                }));
                    }

                    currentKeyItem = new KeyItem
                    {
                        ApplicationName = windowChanged.WindowTitle,
                        Timestamp = windowChanged.Timestamp
                    };
                    newContent.Add(currentKeyItem);
                }
            }

            KeyLogContent = new ObservableCollection<KeyItem>(newContent);
            OnPropertyChanged(nameof(KeyLogContent));
        }

        private string GenerateRawText(List<KeyLogEntry> keyLogEntries)
        {
            var stringBuilder = new StringBuilder();
            var placeholderText = Application.Current.Resources["NoKeyInputs"];
            var entryOpened = false;

            for (int i = 0; i < keyLogEntries.Count; i++)
            {
                var keyLogEntry = keyLogEntries[i];

                var normalTextEntry = keyLogEntry as NormalText;
                if (normalTextEntry != null)
                {
                    if (!string.IsNullOrEmpty(normalTextEntry.Text))
                        stringBuilder.Append(normalTextEntry.Text);
                    continue;
                }

                var specialKey = keyLogEntry as SpecialKey;
                if (specialKey != null)
                {
                    string text;
                    switch (specialKey.KeyType)
                    {
                        case SpecialKeyType.Shift:
                            text = "Shift";
                            break;
                        case SpecialKeyType.Win:
                            text = "Win";
                            break;
                        case SpecialKeyType.Tab:
                            text = "Tab";
                            break;
                        case SpecialKeyType.Captial:
                            text = "Caps";
                            break;
                        case SpecialKeyType.Return:
                            text = "Enter";
                            break;
                        case SpecialKeyType.Back:
                            text = "<-";
                            break;
                        default:
                            continue;
                    }

                    if (HideReleaseKeyState && !specialKey.IsDown)
                        continue;

                    stringBuilder.Append($@"{{{(HideReleaseKeyState ? "" : (specialKey.IsDown ? "+" : "-"))}{text}}}");
                    continue;
                }

                var standardKey = keyLogEntry as StandardKey;
                if (standardKey != null)
                {
                    if (HideReleaseKeyState && !standardKey.IsDown)
                        continue;

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

                    stringBuilder.Append($@"{{{(HideReleaseKeyState ? "" : (standardKey.IsDown ? "+" : "-"))}{text}}}");
                    continue;
                }

                var windowChanged = keyLogEntry as WindowChanged;
                if (windowChanged != null)
                {
                    if (HideEmptyWindows && keyLogEntries.Count - 1 > i && keyLogEntries[i + 1] is WindowChanged)
                        //if next is window change again
                        continue;

                    if (!HideEmptyWindows && i > 0 && keyLogEntries[i - 1] is WindowChanged)
                        stringBuilder.Append($"({placeholderText})");

                    if (entryOpened)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine($"[{windowChanged.Timestamp}] {windowChanged.WindowTitle}");

                    entryOpened = true;
                }
            }

            return stringBuilder.ToString();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}