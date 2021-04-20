using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrcusKeyChecker
{
    public class KeyCheckTask
    {
        private const string BaseUrl = "https://www.orcus.one/Orcus.php";
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isCanceled;

        public KeyCheckTask(Guid licenseKey)
        {
            LicenseKey = licenseKey;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Guid LicenseKey { get; }

        public async Task<KeyCheckResult> CheckKey()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new WebClient())
            {
                _cancellationTokenSource.Token.Register(client.CancelAsync);

                string result;
                try
                {
                    var response =
                        await client.UploadValuesTaskAsync(BaseUrl, new NameValueCollection
                        {
                            {"method", "l"},
                            {"lkey", LicenseKey.ToString("N")}
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

        public void Cancel()
        {
            if (_isCanceled)
                return;

            _isCanceled = true;
            _cancellationTokenSource.Cancel();
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
}