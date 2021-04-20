using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class ArchiveOptionsViewModel : PropertyChangedBase
    {
        private readonly List<EntryInfo> _entries;
        private string _archiveName;
        private RelayCommand _cancelCommand;
        private int _compressionLevel = 5;
        private CompressionMethod _compressionMethod = CompressionMethod.Zip;
        private bool _deleteFilesAfterArchiving;
        private bool? _dialogResult;
        private RelayCommand _okCommand;
        private string _password;
        private bool _useTarAsPacker;

        private readonly Dictionary<CompressionMethod, string> _compressionMethodExtensions =
            new Dictionary<CompressionMethod, string>
            {
                {CompressionMethod.Zip, ".zip"},
                {CompressionMethod.Bzip2, ".bz2"},
                {CompressionMethod.Gzip, ".gz"},
                {CompressionMethod.None, ""}
            };

        public ArchiveOptionsViewModel(List<EntryInfo> entries)
        {
            _entries = entries;
            IsSingleFile = entries.Count == 1 && !entries[0].IsDirectory;
            UpdateArchiveName();
        }

        private void UpdateArchiveName()
        {
            var nameBuilder = new StringBuilder();
            if (IsSingleFile)
                nameBuilder.Append(Path.GetFileName(_entries[0].Path));
            else
                nameBuilder.Append(string.IsNullOrEmpty(ArchiveName) ? Path.GetFileNameWithoutExtension(_entries[0].Path) : ArchiveName);

            var extension = Path.GetExtension(nameBuilder.ToString());
            if (!string.IsNullOrEmpty(extension))
            {
                //remove extension if it's a compression extension
                if (_compressionMethodExtensions.Any(x => x.Value.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                    nameBuilder.Length -= extension.Length;
            }

            if (nameBuilder.Length > 4 && nameBuilder.ToString(nameBuilder.Length - 4, 4) == ".tar")
                nameBuilder.Length -= 4;

            if (UseTarAsPacker)
                nameBuilder.Append(".tar");

            nameBuilder.Append(_compressionMethodExtensions[CompressionMethod]);
            ArchiveName = nameBuilder.ToString();
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public bool IsSingleFile { get; }

        public string ArchiveName
        {
            get { return _archiveName; }
            set { SetProperty(value, ref _archiveName); }
        }

        public CompressionMethod CompressionMethod
        {
            get { return _compressionMethod; }
            set
            {
                if (SetProperty(value, ref _compressionMethod))
                    UpdateArchiveName();
            }
        }

        public bool UseTarAsPacker
        {
            get { return _useTarAsPacker; }
            set
            {
                if (SetProperty(value, ref _useTarAsPacker))
                {
                    if (value)
                        CompressionMethod = CompressionMethod.Gzip;
                    else
                        CompressionMethod = CompressionMethod.Zip;

                    //we must still update
                    UpdateArchiveName();
                }
            }
        }

        public int CompressionLevel
        {
            get { return _compressionLevel; }
            set { SetProperty(value, ref _compressionLevel); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(value, ref _password); }
        }

        public bool DeleteFilesAfterArchiving
        {
            get { return _deleteFilesAfterArchiving; }
            set { SetProperty(value, ref _deleteFilesAfterArchiving); }
        }

        public RelayCommand OkCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand(parameter => { DialogResult = true; })); }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }
    }
}