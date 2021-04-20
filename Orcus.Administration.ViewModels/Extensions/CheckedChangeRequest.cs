using System;

namespace Orcus.Administration.ViewModels.Extensions
{
    public class CheckedChangeRequest
    {
        public object Parameter { get; set; }
        public bool CurrentStatus { get; set; }
        public bool RequestedStatus { get; set; }
        public Action AcceptRequest { get; set; }
    }
}