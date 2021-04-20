using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Core;
using Orcus.Shared.Encryption;
using Orcus.Shared.Server;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;
using FileExtensions = Orcus.Shared.Utilities.FileExtensions;
using StringExtensions = Orcus.Administration.Core.Utilities.StringExtensions;

namespace Orcus.Administration.ViewModels
{
    public class ConfigureServerViewModel : PropertyChangedBase
    {
        private RelayCommand _addIpAddressCommand;
        private RelayCommand _buildServerCommand;
        private bool? _dialogResult;
        private RelayCommand _extractServerCommand;
        private string _ip2LocationEmailAddress;
        private SecureString _ip2LocationPassword;
        private SecureString _password = new SecureString();
        private RelayCommand _registerIp2LocationCommand;
        private RelayCommand _removeIpAddressCommand;

        public ConfigureServerViewModel()
        {
            IpAddresses = new ObservableCollection<IpAddressInfo>();

            var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName())
                .OrderByDescending(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.ToString())
                .ToList();
            ipAddresses.Insert(0, "127.0.0.1");
            AvailableIpAddresses = ipAddresses.ToArray();

            IpAddresses.Add(new IpAddressInfo {Ip = "127.0.0.1", Port = 10134});
            IpAddresses.Add(new IpAddressInfo {Ip = NetworkUtilities.GetLanIp().ToString(), Port = 10134});
        }

        public ObservableCollection<IpAddressInfo> IpAddresses { get; }
        public bool IsGuiSelected { get; set; } = true;

        public string[] AvailableIpAddresses { get; }

        public SecureString Password
        {
            get { return _password; }
            set { SetProperty(value, ref _password); }
        }

        public string Ip2LocationEmailAddress
        {
            get { return _ip2LocationEmailAddress; }
            set { SetProperty(value, ref _ip2LocationEmailAddress); }
        }

        public SecureString Ip2LocationPassword
        {
            get { return _ip2LocationPassword; }
            set { SetProperty(value, ref _ip2LocationPassword); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand RemoveIpAddressCommand
        {
            get
            {
                return _removeIpAddressCommand ?? (_removeIpAddressCommand = new RelayCommand(parameter =>
                {
                    var ipAddress = parameter as IpAddressInfo;
                    if (ipAddress == null)
                        return;

                    IpAddresses.Remove(ipAddress);
                }));
            }
        }

        public RelayCommand AddIpAddressCommand
        {
            get
            {
                return _addIpAddressCommand ?? (_addIpAddressCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var ipAddress = (string) parameters[0];
                    var port = (int) (double) parameters[1];

                    if (!IpAddresses.Any(x => x.Ip == ipAddress && x.Port == port))
                        IpAddresses.Add(new IpAddressInfo {Ip = ipAddress, Port = port});
                    else
                        WindowServiceInterface.Current.ShowMessageBox(
                            (string) Application.Current.Resources["IpAddressAlreadyAdded"],
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

        public RelayCommand RegisterIp2LocationCommand
        {
            get
            {
                return _registerIp2LocationCommand ??
                       (_registerIp2LocationCommand =
                           new RelayCommand(parameter => { Process.Start("https://lite.ip2location.com/sign-up"); }));
            }
        }

        public RelayCommand ExtractServerCommand
        {
            get
            {
                return _extractServerCommand ?? (_extractServerCommand = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog
                    {
                        Title = (string) Application.Current.Resources["PleaseSelectServerLocation"],
                        Filter = $"{Application.Current.Resources["Application"]}|*.exe",
                        FileName = IsGuiSelected ? "Orcus.Server" : "Orcus.Server.CommandLine",
                        AddExtension = true,
                        InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                    };

                    if (!sfd.ShowDialog() == true)
                        return;

                    try
                    {
                        var resource =
                            Application.GetResourceStream(
                                new Uri(
                                    $"pack://application:,,,/Orcus.Administration.Resources;component/Server/{(IsGuiSelected ? "Orcus.Server.exe" : "Orcus.Server.CommandLine.exe")}"));

                        if (resource == null)
                            throw new FileNotFoundException();

                        using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                        using (var stream = resource.Stream)
                            stream.CopyTo(fs);

                        Process.Start("explorer.exe", $"/select, \"{sfd.FileName}\"");
                        DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        public RelayCommand BuildServerCommand
        {
            get
            {
                return _buildServerCommand ?? (_buildServerCommand = new RelayCommand(parameter =>
                {
                    if (IpAddresses.Count == 0)
                        return;

                    if (Password.Length == 0)
                        return;

                    var serverDirectory =
                        new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server"));

                    if (serverDirectory.Exists)
                        serverDirectory = new DirectoryInfo(FileExtensions.MakeDirectoryUnique(serverDirectory.FullName));

                    serverDirectory.Create();

                    var sfd = new SaveFileDialog
                    {
                        Title = (string) Application.Current.Resources["PleaseSelectServerLocation"],
                        Filter = $"{Application.Current.Resources["Application"]}|*.exe",
                        FileName = IsGuiSelected ? "Orcus.Server" : "Orcus.Server.CommandLine",
                        AddExtension = true,
                        InitialDirectory = serverDirectory.FullName
                    };

                    if (!sfd.ShowDialog() == true)
                    {
                        if (!serverDirectory.EnumerateFileSystemInfos().Any())
                            serverDirectory.Delete();
                        return;
                    }

                    var directory = Path.GetDirectoryName(sfd.FileName);
                    if (directory == null)
                    {
                        //we shouldn't reach that point
                        WindowServiceInterface.Current.ShowMessageBox(
                            "The directory of the select location is null. Please contact the developer", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (Directory.Exists(serverDirectory.FullName) && directory != serverDirectory.FullName &&
                        !serverDirectory.EnumerateFileSystemInfos().Any())
                        serverDirectory.Delete(true);

                    try
                    {
                        var certificateFile = new FileInfo(Path.Combine(directory, "certificate.pfx"));
                        if (certificateFile.Exists &&
                            WindowServiceInterface.Current.ShowMessageBox(
                                (string) Application.Current.Resources["CertificateAlreadyExists"],
                                (string) Application.Current.Resources["Warning"], MessageBoxButton.OKCancel,
                                MessageBoxImage.Warning) != MessageBoxResult.OK)
                            return;

                        byte[] certificate;
                        var certificatePassword =
                            System.Web.Security.Membership.GeneratePassword(100 + new Random().Next(-15, 16), 25);

                        try
                        {
                            certificate =
                                Certificate.CreateSelfSignCertificatePfx(
                                    "CN=Orcus Server",
                                    DateTime.Today.AddYears(-1),
                                    DateTime.Today.AddYears(30),
                                    certificatePassword);
                        }
                        catch (Exception ex)
                        {
                            WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                                (string) Application.Current.Resources["Error"],
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        File.WriteAllBytes(certificateFile.FullName, certificate);
                        var config = new ServerConfig
                        {
                            IpAddresses = IpAddresses.ToList(),
                            Password = StringExtensions.SecureStringToString(Password),
                            SslCertificatePath = certificateFile.Name,
                            SslCertificatePassword = AES.Encrypt(certificatePassword, "just trolling the repo")
                        };


                        if (!string.IsNullOrEmpty(Ip2LocationEmailAddress) && Ip2LocationPassword?.Length > 0)
                        {
                            config.IsGeoIpLocationEnabled = true;
                            config.Ip2LocationEmailAddress = Ip2LocationEmailAddress;
                            config.Ip2LocationPassword = StringExtensions.SecureStringToString(Ip2LocationPassword);
                        }

                        File.WriteAllText(Path.Combine(directory, "settings.json"),
                            JsonConvert.SerializeObject(config, Formatting.Indented));

                        var resource =
                            Application.GetResourceStream(
                                new Uri(
                                    $"pack://application:,,,/Orcus.Administration.Resources;component/Server/{(IsGuiSelected ? "Orcus.Server.exe" : "Orcus.Server.CommandLine.exe")}"));

                        if (resource == null)
                            throw new FileNotFoundException();

                        using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                        using (var stream = resource.Stream)
                            stream.CopyTo(fs);

                        Process.Start("explorer.exe", $"/select, \"{sfd.FileName}\"");
                        DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }
    }
}