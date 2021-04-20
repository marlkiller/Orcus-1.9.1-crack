using System;

namespace Orcus.Shared.Commands.Webcam
{
    [Serializable]
    public class WebcamSettings
    {
        public string MonikerString { get; set; }
        public int Resolution { get; set; }
        public int Quality { get; set; }
    }
}