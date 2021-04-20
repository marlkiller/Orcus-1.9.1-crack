using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Administration.Core.Plugins.Web;

namespace Orcus.Administration.ViewModels.ViewInterface
{
    public interface IWebServerConnection
    {
        Task DownloadPlugin(Guid pluginGuid, string fileName, Action<double> reportProgressAction);
        Task<List<PublicPluginData>> GetAllPlugins();
    }
}