using System;

namespace Orcus.Shared.Commands.HVNC
{
    [Serializable]
    public class CreateDesktopInformation
    {
        public bool StartExplorer { get; set; }
        public string CustomName { get; set; }
    }
}