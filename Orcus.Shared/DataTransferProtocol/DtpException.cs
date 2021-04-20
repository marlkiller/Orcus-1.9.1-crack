using System;

namespace Orcus.Shared.DataTransferProtocol
{
    [Serializable]
    public class DtpException
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string FunctionName { get; set; }
        public string ParameterInformation { get; set; }
        public Guid SessionGuid { get; set; }
    }
}