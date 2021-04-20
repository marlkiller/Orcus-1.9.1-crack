using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Administration.Core;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class PropertiesViewModel
    {
        public PropertiesViewModel(DirectoryNodeViewModel directoryViewModel, DirectoryPropertiesInfo directoryProperties)
            : this(directoryViewModel)
        {
            EntryInfo = new DirectoryEntryInfoViewModel(directoryViewModel, directoryProperties);
        }

        public PropertiesViewModel(FileEntryViewModel fileEntryViewModel, FilePropertiesInfo fileProperties,
            FileExplorerCommand fileExplorerCommand)
            : this(fileEntryViewModel)
        {
            EntryInfo = new FileEntryInfoViewModel(fileEntryViewModel, fileProperties, fileExplorerCommand);
        }

        private PropertiesViewModel(IEntryViewModel entryViewModel)
        {
            //Title = string.Format((string)Application.Current.Resources["PropertiesOf"], entryViewModel.Label);
            Icon = entryViewModel.Icon;
        }

        public IEntryInfoViewModel EntryInfo { get; set; }
        public ImageSource Icon { get; }
    }

    public class FileEntryInfoViewModel : PropertyChangedBase, IEntryInfoViewModel
    {
        private readonly FileExplorerCommand _fileExplorerCommand;
        private RelayCommand _calculateHashValuesCommand;
        private RelayCommand _copyAllCommand;
        private RelayCommand _copyHashValueCommand;
        private RelayCommand _copyPropertyCommand;
        private RelayCommand _copyPropertyNameCommand;
        private RelayCommand _copyPropertyValueCommand;
        private bool _isLoadingHashValues;
        private ICollectionView _translatedFileProperties;

        public FileEntryInfoViewModel(FileEntryViewModel fileEntryViewModel, FilePropertiesInfo fileProperties,
            FileExplorerCommand fileExplorerCommand)
        {
            _fileExplorerCommand = fileExplorerCommand;
            FileEntryViewModel = fileEntryViewModel;
            FilePropertiesInfo = fileProperties;
            HashValueInfos = new ObservableCollection<HashValueInfo>();
        }

        public ICollectionView TranslatedFileProperties
        {
            get
            {
                if (_translatedFileProperties == null)
                {
                    var list = new List<FileProperty>();
                    foreach (var fileProperty in FilePropertiesInfo.FileProperties)
                    {
                        string translatedName;

                        if (fileProperty is ShellProperty)
                        {
                            translatedName = ((ShellProperty)fileProperty).GetDisplayName();
                        }
                        else
                            switch (fileProperty.Name)
                            {
                                case "CompanyName":
                                    translatedName = (string)Application.Current.Resources["CompanyName"];
                                    break;
                                case "FileDescription":
                                    translatedName = (string)Application.Current.Resources["Description"];
                                    break;
                                case "FileName":
                                    translatedName = (string)Application.Current.Resources["FileName"];
                                    break;
                                case "FileVersion":
                                    translatedName = (string)Application.Current.Resources["FileVersion"];
                                    break;
                                case "InternalName":
                                    translatedName = (string)Application.Current.Resources["Description"];
                                    break;
                                case "Language":
                                    translatedName = (string)Application.Current.Resources["Language"];
                                    break;
                                case "LegalCopyright":
                                    translatedName = (string)Application.Current.Resources["Copyright"];
                                    break;
                                case "OriginalFilename":
                                    translatedName = (string)Application.Current.Resources["OriginalFilename"];
                                    break;
                                case "ProductName":
                                    translatedName = (string)Application.Current.Resources["Product"];
                                    break;
                                case "ProductVersion":
                                    translatedName = (string)Application.Current.Resources["ProductVersion"];
                                    break;
                                case "IsAssembly":
                                    translatedName = (string)Application.Current.Resources["NetAssembly"];
                                    break;
                                case "AssemblyName":
                                    translatedName = (string)Application.Current.Resources["AssemblyName"];
                                    break;
                                case "IsTrusted":
                                    translatedName = (string)Application.Current.Resources["Trusted"];
                                    break;
                                default:
                                    translatedName = fileProperty.Name;
                                    break;
                            }

                        if (string.IsNullOrEmpty(translatedName))
                            continue;

                        var value = fileProperty.Value;
                        DateTime parsedDateTime;
                        if (DateTime.TryParse(value, out parsedDateTime))
                            value = parsedDateTime.ToLocalTime().ToString("G", Settings.Current.Language.CultureInfo);

                        list.Add(new FileProperty
                        {
                            Name = translatedName,
                            Value = value,
                            Group = fileProperty.Group
                        });
                    }

                    _translatedFileProperties = CollectionViewSource.GetDefaultView(list);
                    _translatedFileProperties.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                    _translatedFileProperties.SortDescriptions.Add(new SortDescription("Name",
                        ListSortDirection.Ascending));
                }
                return _translatedFileProperties;
            }
        }

        public ObservableCollection<HashValueInfo> HashValueInfos { get; }
        public FileEntryViewModel FileEntryViewModel { get; }
        public FilePropertiesInfo FilePropertiesInfo { get; }

        public bool IsLoadingHashValues
        {
            get { return _isLoadingHashValues; }
            set { SetProperty(value, ref _isLoadingHashValues); }
        }

        public RelayCommand CopyPropertyCommand
        {
            get
            {
                return _copyPropertyCommand ?? (_copyPropertyCommand = new RelayCommand(parameter =>
                {
                    var fileProperty = (FileProperty)parameter;
                    Clipboard.SetDataObject($"{fileProperty.Name} = {fileProperty.Value}");
                }));
            }
        }

        public RelayCommand CopyPropertyNameCommand
        {
            get
            {
                return _copyPropertyNameCommand ?? (_copyPropertyNameCommand = new RelayCommand(parameter =>
                {
                    var fileProperty = (FileProperty)parameter;
                    Clipboard.SetDataObject(fileProperty.Name);
                }));
            }
        }

        public RelayCommand CopyPropertyValueCommand
        {
            get
            {
                return _copyPropertyValueCommand ?? (_copyPropertyValueCommand = new RelayCommand(parameter =>
                {
                    var fileProperty = (FileProperty)parameter;
                    Clipboard.SetDataObject(fileProperty.Value);
                }));
            }
        }

        public RelayCommand CopyAllCommand
        {
            get
            {
                return _copyAllCommand ?? (_copyAllCommand = new RelayCommand(parameter =>
                {
                    Clipboard.SetDataObject(
                        string.Join(Environment.NewLine,
                            TranslatedFileProperties.Cast<FileProperty>().Select(x => $"{x.Name} = {x.Value}").ToArray()));
                }));
            }
        }

        public RelayCommand CalculateHashValuesCommand
        {
            get
            {
                return _calculateHashValuesCommand ?? (_calculateHashValuesCommand = new RelayCommand(async parameter =>
                {
                    if (IsLoadingHashValues)
                        return;

                    HashValueInfos.Clear();
                    IsLoadingHashValues = true;
                    var allTypes = new[]
                    {HashValueType.MD5, HashValueType.SHA1, HashValueType.SHA256, HashValueType.SHA512};
                    foreach (var hashValueType in allTypes)
                    {
                        byte[] value;
                        try
                        {
                            value =
                                await
                                    Task.Run(
                                        () => _fileExplorerCommand.ComputeHash(EntryViewModel.Value.Path, hashValueType));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, (string)Application.Current.Resources["Error"],
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        HashValueInfos.Add(new HashValueInfo
                        {
                            Type = hashValueType.ToString(),
                            Value = BitConverter.ToString(value).Replace("-", null)
                        });
                    }
                    IsLoadingHashValues = false;
                }));
            }
        }

        public RelayCommand CopyHashValueCommand
        {
            get
            {
                return _copyHashValueCommand ?? (_copyHashValueCommand = new RelayCommand(parameter =>
                {
                    var hashValue = (HashValueInfo)parameter;
                    Clipboard.SetDataObject(hashValue.Value);
                }));
            }
        }

        public IEntryViewModel EntryViewModel => FileEntryViewModel;
        public PropertiesInfo PropertiesInfo => FilePropertiesInfo;
        public bool IsDirectoryInfo { get; } = false;
        public string Attributes => PropertiesInfo.Attributes.ToString();
    }

    public class DirectoryEntryInfoViewModel : IEntryInfoViewModel
    {
        public DirectoryEntryInfoViewModel(DirectoryNodeViewModel directoryNodeViewModel,
            DirectoryPropertiesInfo directoryPropertiesInfo)
        {
            DirectoryNodeViewModel = directoryNodeViewModel;
            DirectoryPropertiesInfo = directoryPropertiesInfo;
        }

        public long AvailableSpace
        {
            get
            {
                var driveDirectory = DirectoryNodeViewModel.Value as DriveDirectoryEntry;
                if (driveDirectory == null)
                    return 0;
                return driveDirectory.TotalSize - driveDirectory.UsedSpace;
            }
        }

        public bool LabelIsDifferent => DirectoryNodeViewModel.Label != DirectoryNodeViewModel.Name;
        public DirectoryNodeViewModel DirectoryNodeViewModel { get; }
        public DirectoryPropertiesInfo DirectoryPropertiesInfo { get; }
        public IEntryViewModel EntryViewModel => DirectoryNodeViewModel;
        public PropertiesInfo PropertiesInfo => DirectoryPropertiesInfo;
        public bool IsDirectoryInfo { get; } = true;
        public string Attributes => PropertiesInfo.Attributes.ToString();
    }

    public class HashValueInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public interface IEntryInfoViewModel
    {
        IEntryViewModel EntryViewModel { get; }
        PropertiesInfo PropertiesInfo { get; }
        bool IsDirectoryInfo { get; }
        string Attributes { get; }
    }
}