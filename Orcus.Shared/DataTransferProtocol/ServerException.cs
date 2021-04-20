using System;
using System.Reflection;

namespace Orcus.Shared.DataTransferProtocol
{
    public class ServerException : Exception
    {
        public ServerException(DtpException exception)
            : base(
                $"Remote connection threw an error when executing function \"{exception.FunctionName}\": {exception.Message}", new Exception(exception.Message)
                )
        {
            ServerMessage =
                $"Function name: {exception.FunctionName}\r\nParameters: {(string.IsNullOrEmpty(exception.ParameterInformation) ? "null" : exception.ParameterInformation)}\r\n" +
                exception.StackTrace + "\r\n--------- FINISH SERVER STACK TRACE ---------\r\n";

            var field = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(this, ServerMessage); //change stack trace with a small hack
        }

        public string ServerMessage { get; }
    }
}