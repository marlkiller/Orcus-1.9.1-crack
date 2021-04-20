using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using AudioPackBuilder.Build;
using AudioPackBuilder.Core;
using Microsoft.Win32;
using Mono.Cecil;
using Orcus.Plugins;
using Sorzus.Wpf.Toolkit;

namespace AudioPackBuilder.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private RelayCommand _addAudioFileCommand;
        private RelayCommand _changeAudioThumbnailCommand;
        private RelayCommand _createPluginCommand;
        private RelayCommand _removeAudioFilesCommand;
        private RelayCommand _removeAudioThumbnailCommand;
        private RelayCommand _selectThumbnailPathCommand;
        private string _thumbnailPath;

        public MainViewModel()
        {
            AudioFiles = new ObservableCollection<AudioFile>();
            PluginInfo = new PluginInfo {PluginType = PluginType.Audio};
        }

        public string ThumbnailPath
        {
            get { return _thumbnailPath; }
            set { SetProperty(value, ref _thumbnailPath); }
        }

        public PluginInfo PluginInfo { get; }

        public ObservableCollection<AudioFile> AudioFiles { get; }

        public RelayCommand SelectThumbnailPathCommand
        {
            get
            {
                return _selectThumbnailPathCommand ?? (_selectThumbnailPathCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog {Filter = "Image file|*.jpg;*.png"};
                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                        ThumbnailPath = ofd.FileName;
                }));
            }
        }

        public RelayCommand AddAudioFileCommand
        {
            get
            {
                return _addAudioFileCommand ?? (_addAudioFileCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog {Filter = "MP3 files|*.mp3", Multiselect = true};
                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        try
                        {
                            foreach (var fileName in ofd.FileNames)
                                AudioFiles.Add(new AudioFile(fileName));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred: \"{ex.Message}\"");
                        }
                    }
                }));
            }
        }

        public RelayCommand ChangeAudioThumbnailCommand
        {
            get
            {
                return _changeAudioThumbnailCommand ?? (_changeAudioThumbnailCommand = new RelayCommand(parameter =>
                {
                    var audioFile = parameter as AudioFile;
                    if (audioFile == null)
                        return;

                    var ofd = new OpenFileDialog
                    {
                        Title = "Please select a new thumbnail (60x35)",
                        Filter = "JPG file|*.jpg"
                    };
                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        audioFile.ThumbnailPath = ofd.FileName;
                    }
                }));
            }
        }

        public RelayCommand RemoveAudioThumbnailCommand
        {
            get
            {
                return _removeAudioThumbnailCommand ?? (_removeAudioThumbnailCommand = new RelayCommand(parameter =>
                {
                    var audioFile = parameter as AudioFile;
                    if (audioFile != null)
                        audioFile.ThumbnailPath = null;
                }));
            }
        }

        public string Version { get; set; } = "1.0";
        public string PluginGuid { get; set; } = Guid.NewGuid().ToString("D");

        public RelayCommand RemoveAudioFilesCommand
        {
            get
            {
                return _removeAudioFilesCommand ?? (_removeAudioFilesCommand = new RelayCommand(parameter =>
                {
                    var audioFiles = ((IList) parameter).Cast<AudioFile>().ToList();
                    foreach (var audioFile in audioFiles)
                        AudioFiles.Remove(audioFile);
                }));
            }
        }

        public RelayCommand CreatePluginCommand
        {
            get
            {
                return _createPluginCommand ?? (_createPluginCommand = new RelayCommand(parameter =>
                {
                    PluginVersion pluginVersion;
                    Guid guid;

                    if (!PluginVersion.TryParse(Version, out pluginVersion))
                    {
                        MessageBox.Show("Invalid version. Please use the format \"0.0\".", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    if (!Guid.TryParse(PluginGuid, out guid))
                    {
                        MessageBox.Show("Invalid guid format.", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    PluginInfo.Guid = guid;
                    PluginInfo.Version = pluginVersion;

                    var resource =
                        Application.GetResourceStream(
                            new Uri(
                                "/Resources/AudioPackBuilder.Plugin.dll", UriKind.Relative));

                    if (resource == null)
                        throw new FileNotFoundException();

                    using (var stream = resource.Stream)
                    {
                        var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream);
                        var list = new List<RawAudioFile>();
                        foreach (var audioFile in AudioFiles)
                        {
                            var audioFileName = Guid.NewGuid().ToString("N");
                            var rawAudioFile = new RawAudioFile
                            {
                                AudioGenre = audioFile.AudioGenre,
                                Name = audioFile.Name,
                                Timespan = XmlConvert.ToString(audioFile.Duration)
                            };

                            try
                            {
                                if (!string.IsNullOrEmpty(audioFile.ThumbnailPath))
                                {
                                    var thumbnailName = audioFileName + "_t";
                                    assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource(thumbnailName,
                                        ManifestResourceAttributes.Private, File.ReadAllBytes(audioFile.ThumbnailPath)));
                                    rawAudioFile.EmbeddedThumbnailResourceName = thumbnailName;
                                }

                                assemblyDefinition.MainModule.Resources.Add(new EmbeddedResource(audioFileName,
                                    ManifestResourceAttributes.Private, File.ReadAllBytes(audioFile.Path)));
                                rawAudioFile.EmbeddedResourceName = audioFileName;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error while adding audio file \"{audioFile.Name}\": {ex.Message}");
                                return;
                            }
                            list.Add(rawAudioFile);
                        }

                        assemblyDefinition.Modules[0].Types.First(
                            x => x.FullName == "AudioPackBuilder.Plugin.DataProvider")
                            .Methods.First(x => x.Name == ".cctor")
                            .Body.Instructions.First(x => x.OpCode.Name == "ldstr")
                            .Operand = new JavaScriptSerializer().Serialize(list);

                        using (var assemblyStream = new MemoryStream())
                        {
                            assemblyDefinition.Write(assemblyStream);
                            assemblyStream.Position = 0;

                            var sfd = new SaveFileDialog
                            {
                                Filter = "Audio Pack|*.orcplg",
                                FileName = PluginInfo.Name.Replace(" ", null)
                            };

                            if (sfd.ShowDialog(Application.Current.MainWindow) != true)
                                return;

                            using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
                            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                            {
                                if (!string.IsNullOrEmpty(ThumbnailPath))
                                {
                                    var thumbnail = "thumbnail" + Path.GetExtension(ThumbnailPath);
                                    var thumbnailEntry = archive.CreateEntry(thumbnail,
                                        CompressionLevel.Optimal);
                                    WriteFileToArchiveEntry(thumbnailEntry, ThumbnailPath);
                                    PluginInfo.Thumbnail = thumbnailEntry.Name;
                                }

                                var libraryEntry = archive.CreateEntry("AudioPack.dll", CompressionLevel.Optimal);
                                using (var archiveStream = libraryEntry.Open())
                                {
                                    assemblyStream.WriteTo(archiveStream);
                                }

                                PluginInfo.Library1 = "AudioPack.dll";

                                var infoFile = archive.CreateEntry("PluginInfo.xml", CompressionLevel.Optimal);
                                using (var infoStream = infoFile.Open())
                                {
                                    var xmls = new XmlSerializer(typeof (PluginInfo));
                                    xmls.Serialize(infoStream, PluginInfo);
                                }
                            }

                            Process.Start("explorer.exe", $"/select, \"{sfd.FileName}\"");
                        }
                    }
                }));
            }
        }

        private void WriteFileToArchiveEntry(ZipArchiveEntry archiveEntry, string path)
        {
            using (var thumbnailStream = archiveEntry.Open())
            using (
                var localThumbnailStream = new FileStream(path, FileMode.Open, FileAccess.Read)
                )
                localThumbnailStream.CopyTo(thumbnailStream);
        }
    }
}