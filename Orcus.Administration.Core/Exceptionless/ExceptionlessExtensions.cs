using System.Net;
using Exceptionless;

namespace Orcus.Administration.Core.Exceptionless
{
    public static class ExceptionlessExtensions
    {
        public static void SubmitNotFoundWithCheck(this ExceptionlessClient client, string url)
        {
            try
            {
                using (var webClient = new WebClient())
                using (var stream = webClient.OpenRead("http://www.google.com"))
                {
                    client.SubmitNotFound(url);
                }
            }
            catch
            {
                //Client just has no internet connection
            }
        }
    }
}