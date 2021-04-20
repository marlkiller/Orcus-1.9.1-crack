using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Orcus.Plugins;

namespace PluginCreator
{
    public partial class MainForm : Form
    {
        private readonly Settings _settings;

        public MainForm()
        {
            InitializeComponent();
            GuidTextBox.Text = Guid.NewGuid().ToString("D").ToUpper();

            var settingsFile = new FileInfo("settings.xml");
            _settings = settingsFile.Exists
                ? Settings.LoadSettings(settingsFile.FullName)
                : new Settings(settingsFile.FullName);
        }

        private void CommandViewRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RefreshLibrariesState();
        }

        private void CommandFactoryRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RefreshLibrariesState();
        }

        private void RefreshLibrariesState()
        {
            CommandPanel.Visible = RequiresTwoLibraries();
            LibraryLabel.Text = RequiresTwoLibraries() ? "View Library" : "Library";
        }

        private void SelectThumbnailPathButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog {Filter = "Image|*.jpg;*.png"})
                if (ofd.ShowDialog(this) == DialogResult.OK)
                    ThumbnailTextBox.Text = ofd.FileName;
        }

        private void SelectLibraryButton_Click(object sender, EventArgs e)
        {
            string path;
            if (GetLibraryPath(out path))
                LibraryTextBox.Text = path;
        }

        private void SelectLibrary2Button_Click(object sender, EventArgs e)
        {
            string path;
            if (GetLibraryPath(out path))
                Library2TextBox.Text = path;
        }

        private bool GetLibraryPath(out string path)
        {
            path = null;
            using (var ofd = new OpenFileDialog {Filter = "Library|*.dll"})
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    path = ofd.FileName;
                    return true;
                }

            return false;
        }

        private void BuildButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please type a name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Please type in a description", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(AuthorTextBox.Text))
            {
                MessageBox.Show("Please type in an author", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Guid garcon;
            if (!Guid.TryParse(GuidTextBox.Text, out garcon))
            {
                MessageBox.Show("Please type a valid guid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(VersionTextBox.Text))
            {
                MessageBox.Show("Please type in a version", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(LibraryTextBox.Text))
            {
                MessageBox.Show("Please select the library", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(LibraryTextBox.Text))
            {
                MessageBox.Show("The library doesn't exsit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RequiresTwoLibraries() && string.IsNullOrWhiteSpace(Library2TextBox.Text))
            {
                MessageBox.Show("Please select the command library", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (RequiresTwoLibraries() && !File.Exists(Library2TextBox.Text))
            {
                MessageBox.Show("The command library doesn't exsit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Path.GetFileName(LibraryTextBox.Text) == Path.GetFileName(Library2TextBox.Text))
            {
                MessageBox.Show("The libraries names are equal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FileInfo thumbnailFile = null;

            if (!string.IsNullOrEmpty(ThumbnailTextBox.Text))
            {
                thumbnailFile = new FileInfo(ThumbnailTextBox.Text);
                if (!thumbnailFile.Exists)
                {
                    MessageBox.Show("The thumbnail doesn't exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var pluginInfo = new PluginInfo
            {
                Name = NameTextBox.Text,
                Author = AuthorTextBox.Text,
                AuthorUrl = AuthorUrlTextBox.Text,
                Description = DescriptionTextBox.Text,
                Guid = garcon
            };

            PluginVersion version;
            if (!PluginVersion.TryParse(VersionTextBox.Text, out version))
            {
                MessageBox.Show("Invalid version", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pluginInfo.Version = version;

            if (AudioRadioButton.Checked)
                pluginInfo.PluginType = PluginType.Audio;
            else if (BuildRadioButton.Checked)
                pluginInfo.PluginType = PluginType.Build;
            else if (ClientRadioButton.Checked)
                pluginInfo.PluginType = PluginType.Client;
            else if (CommandViewRadioButton.Checked)
                pluginInfo.PluginType = PluginType.CommandView;
            else if (ViewRadioButton.Checked)
                pluginInfo.PluginType = PluginType.View;
            else if (AdministrationRadioButton.Checked)
                pluginInfo.PluginType = PluginType.Administration;
            else if (CommandFactoryRadioButton.Checked)
                pluginInfo.PluginType = PluginType.CommandFactory;

            var sfd = new SaveFileDialog
            {
                Filter = "Orcus Plugin|*.orcplg",
                FileName = NameTextBox.Text.Replace(" ", null)
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                if (thumbnailFile != null)
                {
                    var thumbnailEntry = archive.CreateEntry("thumbnail" + thumbnailFile.Extension,
                        CompressionLevel.Optimal);
                    WriteFileToArchiveEntry(thumbnailEntry, thumbnailFile.FullName);
                    pluginInfo.Thumbnail = thumbnailEntry.Name;
                }

                var libraryFile = new FileInfo(LibraryTextBox.Text);
                var libraryEntry = archive.CreateEntry(libraryFile.Name, CompressionLevel.Optimal);
                WriteFileToArchiveEntry(libraryEntry, libraryFile.FullName);
                pluginInfo.Library1 = libraryEntry.Name;

                if (RequiresTwoLibraries())
                {
                    var libraryFile2 = new FileInfo(Library2TextBox.Text);
                    var libraryEntry2 = archive.CreateEntry(libraryFile2.Name, CompressionLevel.Optimal);
                    WriteFileToArchiveEntry(libraryEntry2, libraryFile2.FullName);
                    pluginInfo.Library2 = libraryEntry2.Name;
                }

                var infoFile = archive.CreateEntry("PluginInfo.xml", CompressionLevel.Optimal);
                using (var infoStream = infoFile.Open())
                {
                    var xmls = new XmlSerializer(typeof (PluginInfo));
                    xmls.Serialize(infoStream, pluginInfo);
                }
            }

            var data = new PluginData
            {
                Author = pluginInfo.Author,
                AuthorUrl = pluginInfo.AuthorUrl,
                Description = pluginInfo.Description,
                Guid = pluginInfo.Guid,
                Library1Path = LibraryTextBox.Text,
                Library2Path = Library2TextBox.Text,
                Name = pluginInfo.Name,
                PluginType = pluginInfo.PluginType,
                Version = VersionTextBox.Text,
                ThumbnailPath = ThumbnailTextBox.Text
            };

            if (_settings.PluginData.Any(x => x.Library1Path == LibraryTextBox.Text))
            {
                _settings.PluginData.Remove(_settings.PluginData.First(x => x.Library1Path == LibraryTextBox.Text));
            }

            _settings.PluginData.Add(data);
            _settings.Save();

            Process.Start("explorer.exe", $"/select, \"{sfd.FileName}\"");
        }

        private bool RequiresTwoLibraries()
        {
            return CommandViewRadioButton.Checked || CommandFactoryRadioButton.Checked;
        }

        private void WriteFileToArchiveEntry(ZipArchiveEntry archiveEntry, string path)
        {
            using (var thumbnailStream = archiveEntry.Open())
            using (
                var localThumbnailStream = new FileStream(path, FileMode.Open, FileAccess.Read)
                )
                localThumbnailStream.CopyTo(thumbnailStream);
        }

        private void LoadPluginData(PluginData data)
        {
            NameTextBox.Text = data.Name;
            AuthorTextBox.Text = data.Author;
            AuthorUrlTextBox.Text = data.AuthorUrl;
            DescriptionTextBox.Text = data.Description;
            GuidTextBox.Text = data.Guid.ToString("D").ToUpper();
            VersionTextBox.Text = data.Version;

            var thumbnailPath = new FileInfo(data.ThumbnailPath);
            if (thumbnailPath.Exists)
                ThumbnailTextBox.Text = thumbnailPath.FullName;

            var library1Path = new FileInfo(data.Library1Path);
            if (library1Path.Exists)
                LibraryTextBox.Text = library1Path.FullName;

            if (!string.IsNullOrEmpty(data.Library2Path))
            {
                var library2Path = new FileInfo(data.Library2Path);
                if (library2Path.Exists)
                    Library2TextBox.Text = library2Path.FullName;
            }

            switch (data.PluginType)
            {
                case PluginType.Audio:
                    AudioRadioButton.Checked = true;
                    break;
                case PluginType.Build:
                    BuildRadioButton.Checked = true;
                    break;
                case PluginType.Client:
                    ClientRadioButton.Checked = true;
                    break;
                case PluginType.CommandView:
                    CommandViewRadioButton.Checked = true;
                    break;
                case PluginType.Administration:
                    AdministrationRadioButton.Checked = true;
                    break;
                case PluginType.View:
                    ViewRadioButton.Checked = true;
                    break;
                case PluginType.CommandFactory:
                    CommandFactoryRadioButton.Checked = true;
                    break;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_settings.PluginData.Count > 0)
            {
                var window = new LoadPluginForm(_settings);
                if (window.ShowDialog(this) == DialogResult.OK)
                    LoadPluginData(window.SelectedPluginData);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }
    }
}