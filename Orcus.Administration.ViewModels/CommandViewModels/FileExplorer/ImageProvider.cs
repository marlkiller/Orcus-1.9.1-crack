using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.Utilities;
using Orcus.Shared.Commands.FileExplorer;
#if DEBUG

#endif

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class ImageProvider
    {
        private readonly Dictionary<int, string> _images;
        private readonly Dictionary<int, BitmapImage> _cachedImages;
        private readonly Dictionary<string, FileInformation> _cachedFileInformation = new Dictionary<string, FileInformation>();

        public ImageProvider()
        {
            _images = new Dictionary<int, string>
            {
                //Drives
                {56, "Drives/CD-DVD.png"},
                {38, "Drives/CD-DVD.png"},
                {39, "Drives/CD-DVD.png"},
                {40, "Drives/CD-DVD.png"},
                {41, "Drives/CD-DVD.png"},
                {61, "Drives/CD-DVD.png"},
                {36, "Drives/DriveWindows.png"},
                {32, "Drives/Drive.png"},
                {33, "Drives/NetworkDrive.png"},
                {31, "Drives/NetworkDriveDC.png"},
                {37, "Drives/Drive.png"}, //{37, "Drives/OpticalDrive.png"},
                {30, "Drives/Drive.png"}, //{30, "Drives/OpticalDrive.png"},
                {35, "Drives/USB.png"},
                //Folders
                {-1, "Folders/AdobeCloud.png"},
                {178, "Folders/Contacts.png"},
                {181, "Folders/Contacts.png"},
                {183, "Folders/DesktopFolder.png"},
                {112, "Folders/Documents.png"},
                {184, "Folders/Download.png"},
                {-2, "Folders/Dropbox.png"},
                {115, "Folders/Favorite.png"},
                //{5304, "Folders/FilesFolder.png"},
                //{129, "Folders/Fonts.png"},
                //{77, "Folders/Fonts.png"},
                {3, "Folders/Folder.png"},
                {4, "Folders/Folder.png"},
                {186, "Folders/Game.png"},
                {-3, "Folders/GoogleDrive.png"},
                {185, "Folders/Links.png"},
                {108, "Folders/Music.png"},
                {73, "Folders/NetworkFolder.png"},
                {74, "Folders/NetworkFolder.png"},
                {1040, "Folders/OneDrive.png"},
                {1043, "Folders/OneDrive.png"},
                {113, "Folders/Pictures.png"},
                {117, "Folders/RecentPlaces.png"},
                {18, "Folders/Search.png"},
                {1025, "Folders/Search.png"},
                {123, "Folders/User.png"},
                {189, "Folders/Videos.png"},
                //Other
                {110, "Folders/DesktopFolder.png"},
                //{1013, "Other/HomeGroup.png"},
                {109, "Other/ThisPC.png"},
                {54, "Other/TrashFull.png"},
                {55, "Other/TrashFull.png"}
            };

            _cachedImages = new Dictionary<int, BitmapImage>();
        }

        private BitmapImage LoadImage(int id)
        {
            BitmapImage cachedImage;
            if (_cachedImages.TryGetValue(id, out cachedImage))
                return cachedImage;

            var image = new BitmapImage(new Uri($"pack://application:,,,/Resources/Images/FileExplorer/{_images[id]}", UriKind.Absolute));
            image.Freeze();
            _cachedImages.Add(id, image);
            return image;
        }

        public BitmapImage GetFolderImage(string folderName, int iconId)
        {
            if (iconId != 0 && _images.ContainsKey(iconId))
                return LoadImage(iconId);
#if DEBUG
            if (iconId != 0)
            {
                Debug.Print("Unknown icon id: " + iconId);
            }
#endif
            switch (folderName.ToUpper())
            {
                case "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}":
                    return LoadImage(109);
            }

            switch (folderName.ToUpper())
            {
                case "DROPBOX":
                    return LoadImage(-2);
                case "ONEDRIVE":
                    return LoadImage(1040);
                case "CREATIVE CLOUD FILES":
                    return LoadImage(-1);
                case "GOOGLE DRIVE":
                    return LoadImage(-3);
            }

            return LoadImage(3);
        }

        public BitmapImage GetFolderImage(PackedDirectoryEntry directoryEntry)
        {
            return GetFolderImage(directoryEntry.Name, Math.Abs(directoryEntry.IconId));
        }

        public ImageSource GetFileImage(FileEntry fileEntry)
        {
            return GetFileImage(fileEntry.Path);
        }

        public ImageSource GetFileImage(string filename)
        {
            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
                return null;

            if (_cachedFileInformation.ContainsKey(extension))
                return _cachedFileInformation[extension].Icon;

            var info = FileExtensions.GetFileTypeDescription(filename);
            _cachedFileInformation.Add(extension, info);

            return info.Icon;
        }

        public string GetFileDescription(FileEntry fileEntry)
        {
            var extension = Path.GetExtension(fileEntry.Name);
            if (string.IsNullOrEmpty(extension))
                return null;

            if (_cachedFileInformation.ContainsKey(extension))
                return _cachedFileInformation[extension].Description;

            var info = FileExtensions.GetFileTypeDescription(fileEntry.Name);
            _cachedFileInformation.Add(extension, info);

            return info.Description;
        }
    }
}