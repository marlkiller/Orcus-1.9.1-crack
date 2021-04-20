using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Exceptionless;
using Orcus.Administration.Core.Exceptionless;
using Orcus.Administration.Core.Plugins.Web;
using Orcus.Administration.Licensing;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.ViewInterface;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for PluginsWindow.xaml
    /// </summary>
    public partial class PluginsWindow
    {
        public static void Initialize()
        {
            PluginsViewModel.WebServerConnection = WebserverConnection.Current;
            PluginsViewModel.DownloadPluginInformation = DownloadPluginInformation;
        }

        public PluginsWindow()
        {
            InitializeComponent();
        }

        public static async Task<List<PublicPluginData>> DownloadPluginInformation(IWindow window)
        {
            try
            {
                return await WebserverConnection.Current.GetAllPlugins();
            }
            catch (Exception ex)
            {
                if (ex is UnregisteredComputerException)
                {
                    File.Delete("license.orcus");
                    window.ShowMessageBox(
                        $"Your computer couldn't be validated. Please register Orcus with your license key again. Response: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }

                if (ex is WebException)
                {
                    new Thread(
                        () => { ExceptionlessClient.Default.SubmitNotFoundWithCheck(WebserverConnection.BaseUrl); })
                    {IsBackground = true}.Start();
                }

                return new List<PublicPluginData>();
            }
        }
    }
}