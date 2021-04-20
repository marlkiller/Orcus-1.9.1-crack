using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Serializable]
    public class RemoteDesktopInformation
    {
        public List<ScreenInfo> Screens { get; set; }
        public CaptureType AvailableCaptureTypes { get; set; }
    }
}