using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Orcus.Business.Manager.Core.Data;
using Sorzus.Wpf.Toolkit.Converter;

namespace Orcus.Business.Manager.Core
{
    public static class WebServerConnection
    {
        private const string BaseUrl = "https://www.orcus.one/Orcus.php";
        public static string Token;

        public static async Task<DatabaseInfo> DownloadData(ICurrentStatusReporter currentStatusReporter)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest) WebRequest.Create(BaseUrl);
            var postData = $"method=a&token={Token}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.KeepAlive = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var output = new MemoryStream())
            {
                var buffer = new byte[2048]; // read in chunks of 2KB
                int bytesRead;
                int downloadedBytes = 0;
                var lastUpdate = DateTime.Now;
                int dataDownloadedSinceLastUpdate = 0;
                double currentSpeed = 0;

                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;
                    dataDownloadedSinceLastUpdate += bytesRead;
                    if (DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(100) || currentSpeed == 0)
                    {
                        var period = DateTime.Now - lastUpdate;
                        currentSpeed = dataDownloadedSinceLastUpdate/period.TotalSeconds;

                        lastUpdate = DateTime.Now;
                        dataDownloadedSinceLastUpdate = 0;
                    }

                    currentStatusReporter.CurrentStatus =
                        $"{FormatBytesConverter.BytesToString(downloadedBytes)} downloaded ({currentSpeed/1024} KiB/s)";
                }
                var result = Encoding.UTF8.GetString(output.ToArray());
                return new JavaScriptSerializer().Deserialize<DatabaseInfo>(result);
            }
        }

        public static async Task<List<License>> GenerateLicenses(int amount, string comment)
        {
            using (var client = new WebClient())
            {
                var response =
                    await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                    {
                        {"method", "ag"},
                        {"token", Token},
                        {"amount", amount.ToString()},
                        {"comment", comment}
                    });

                var result = Encoding.UTF8.GetString(response);
                return new JavaScriptSerializer().Deserialize<List<License>>(result);
            }
        }

        public static async Task<bool> ChangeBanValueLicense(List<License> licenses, bool ban)
        {
            using (var client = new WebClient())
            {
                var response =
                    await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                    {
                        {"method", "ab"},
                        {"token", Token},
                        {
                            "licenses",
                            licenses.Aggregate(new StringBuilder(),
                                (builder, license) => builder.Append(license.id + ";")).ToString()
                        },
                        {"ban", ban.ToString().ToLower()}
                    });

                var result = Encoding.UTF8.GetString(response);
                return result == "0";
            }
        }

        public static async Task<bool> RemoveLicenses(List<License> licenses)
        {
            using (var client = new WebClient())
            {
                var response =
                    await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                    {
                        {"method", "ad"},
                        {"token", Token},
                        {
                            "licenses",
                            licenses.Aggregate(new StringBuilder(),
                                (builder, license) => builder.Append(license.id + ";")).ToString()
                        }
                    });

                var result = Encoding.UTF8.GetString(response);
                return result == "0";
            }
        }

        public static async Task<bool> SetLicensesComment(List<License> licenses, string comment)
        {
            using (var client = new WebClient())
            {
                var response =
                    await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                    {
                        {"method", "ac"},
                        {"token", Token},
                        {
                            "licenses",
                            licenses.Aggregate(new StringBuilder(),
                                (builder, license) => builder.Append(license.id + ";")).ToString()
                        },
                        {"comment", comment}
                    });

                var result = Encoding.UTF8.GetString(response);
                return result == "0";
            }
        }

        public static async Task<bool> ClearComputers(List<License> licenses)
        {
            using (var client = new WebClient())
            {
                var response =
                    await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                    {
                        {"method", "ar"},
                        {"token", Token},
                        {
                            "licenses",
                            licenses.Aggregate(new StringBuilder(),
                                (builder, license) => builder.Append(license.id + ";")).ToString()
                        }
                    });

                var result = Encoding.UTF8.GetString(response);
                return result == "0";
            }
        }
    }
}