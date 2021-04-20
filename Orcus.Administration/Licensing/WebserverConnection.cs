using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Exceptionless;
using Exceptionless.Models;
using Newtonsoft.Json;
using Orcus.Administration.Core.Exceptionless;
using Orcus.Administration.Core.Plugins.Web;
using Orcus.Administration.ViewModels.ViewInterface;

namespace Orcus.Administration.Licensing
{
    public class WebserverConnection : IWebServerConnection
    {
        public const string BaseUrl = "https://orcus.pw/orcusapp/OrcusServer.php";
        private static WebserverConnection _webserverConnection;

        private WebserverConnection()
        {
        }

        public static WebserverConnection Current => _webserverConnection ?? (_webserverConnection = new WebserverConnection())
            ;

        public async Task<List<PublicPluginData>> GetAllPlugins()
        {
            using (var webClient = new WebClient())
            {
                var result =
                    await
                        webClient.DownloadStringTaskAsync(BaseUrl + $"?method=p&hwid={HardwareIdGenerator.HardwareId}");

                int errorCode;
                if (int.TryParse(result, out errorCode))
                {
                    ExceptionlessClient.Default.SubmitEvent(new Event
                    {
                        Message = $"Error while trying to receive plugins: {errorCode}",
                        Source = "WebserverConnection"
                    });
                    switch (errorCode)
                    {
                        case -2:
                            throw new UnregisteredComputerException("Hardware ID was not found");
                        case -3:
                            throw new UnregisteredComputerException("No license found");
                        case -4:
                            throw new UnregisteredComputerException("Your license was banned");
                        default:
                            throw new Exception("Web Response: " + errorCode);
                    }
                }

                return JsonConvert.DeserializeObject<List<PublicPluginData>>(result);
            }
        }

        public async Task<LicenseRequestResult> TryRegister(Guid licenseKey)
        {
            string result;

            var url = "https://www.orcus.one/Orcus.php";

            try
            {
                using (var webClient = new WebClient {Encoding = Encoding.UTF8})
                {
                    var parameters = new NameValueCollection
                    {
                        {"hwid", await Task.Run(() => HardwareIdGenerator.HardwareId)},
                        {"lkey", licenseKey.ToString("N")},
                        {"method", "r"}
                    };

                    result = Encoding.UTF8.GetString(
                        await
                            webClient.UploadValuesTaskAsync(
                                new Uri(url, UriKind.Absolute), "POST",
                                parameters));
                }
            }
            catch (WebException)
            {
                ExceptionlessClient.Default.SubmitNotFoundWithCheck(url);
                return LicenseRequestResult.ServerUnavailable;
            }

            var resultArray = result.Split(new[] {'|'}, 2);
            var licenseResult = (LicenseRequestResult) int.Parse(resultArray[0]);
            if (licenseResult == LicenseRequestResult.Success)
                File.WriteAllText("license.orcus", resultArray[1]);

            return (LicenseRequestResult) int.Parse(resultArray[0]);
        }

        public async Task DownloadPlugin(Guid pluginGuid, string fileName, Action<double> reportProgressAction)
        {
            var request =
                (HttpWebRequest)
                    WebRequest.Create(BaseUrl +
                                      $"?method=d&guid={pluginGuid.ToString("N")}&hwid={HardwareIdGenerator.HardwareId}");

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var output = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[4096]; // read in chunks of 4KiB
                int bytesRead;
                double downloadedBytes = 0;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;
                    reportProgressAction(downloadedBytes/response.ContentLength);
                }
            }
        }

        public async Task<KeyCheckResult> CheckKey()
        {
#if DEBUG
            return KeyCheckResult.Valid;
#endif
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new WebClient())
            {
                string result;
                try
                {
                    var response =
                        await client.UploadValuesTaskAsync("https://www.orcus.one/Orcus.php", new NameValueCollection
                        {
                            {"method", "v"},
                            {"hwid", HardwareIdGenerator.HardwareId}
                        });
                    result = Encoding.UTF8.GetString(response);
                }
                catch (WebException)
                {
                    return KeyCheckResult.ConnectionFailed;
                }
                catch (Exception)
                {
                    return KeyCheckResult.OperationAborted;
                }

                switch (result)
                {
                    case "-2":
                    case "-3":
                        return KeyCheckResult.NotFound;
                    case "-4":
                        return KeyCheckResult.Banned;
                    case "0":
                        return KeyCheckResult.Valid;
                    default:
                        return KeyCheckResult.UnknownResult;
                }
            }
        }
    }

    public enum KeyCheckResult
    {
        ConnectionFailed,
        OperationAborted,
        NotFound,
        Banned,
        Valid,
        UnknownResult
    }

    public enum LicenseRequestResult
    {
        Success = 0,
        LicenseNotFound = 1,
        LicenseBanned = 2,
        MaximumComputersReached = 3,
        ServerUnavailable
    }

    public class UnregisteredComputerException : Exception
    {
        public UnregisteredComputerException(string message) : base(message)
        {
        }
    }
}