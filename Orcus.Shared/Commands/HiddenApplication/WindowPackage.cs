using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.HiddenApplication
{
    [Serializable]
    public class WindowPackage
    {
        public List<ApplicationWindow> Windows { get; set; }
        public byte[] WindowData { get; set; }
        public long WindowHandle { get; set; }
    }
}