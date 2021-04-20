using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.Webcam
{
    [Serializable]
    public class WebcamInfo
    {
        public string Name { get; set; }
        public string MonikerString { get; set; }
        public List<WebcamResolution> AvailableResolutions { get; set; }
    }
}