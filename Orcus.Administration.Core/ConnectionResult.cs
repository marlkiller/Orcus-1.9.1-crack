namespace Orcus.Administration.Core
{
    public class ConnectionResult
    {
        public ConnectionResult(bool isConnected, string errorMessage)
        {
            IsConnected = isConnected;
            ErrorMessage = errorMessage;
        }

        public ConnectionResult(bool isConnected)
        {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; }
        public string ErrorMessage { get; }
    }
}