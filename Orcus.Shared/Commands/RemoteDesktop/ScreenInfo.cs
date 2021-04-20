using System;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Serializable]
    public class ScreenInfo
    {
        public int Number { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var screenInfo = obj as ScreenInfo;
            return screenInfo?.Number == Number;
        }

        protected bool Equals(ScreenInfo other)
        {
            return Number == other.Number && Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Number;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }
    }
}