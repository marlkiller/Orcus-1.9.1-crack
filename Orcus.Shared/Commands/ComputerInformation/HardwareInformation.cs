using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class HardwareInformation
    {
        public ProcessorInfo ProcessorInfo { get; set; }
        public VideoCardInfo VideoCardInfo { get; set; }
        public List<Screen> Screens { get; set; }
    }
}