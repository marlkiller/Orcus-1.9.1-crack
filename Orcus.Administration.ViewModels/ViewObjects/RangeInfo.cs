namespace Orcus.Administration.ViewModels.ViewObjects
{
    public struct RangeInfo
    {
        public RangeInfo(int startIndex, int count)
        {
            StartIndex = startIndex;
            Count = count;
        }

        public int StartIndex { get; }
        public int Count { get; }

        public bool Equals(RangeInfo other)
        {
            return StartIndex == other.StartIndex && Count == other.Count;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartIndex * 397) ^ Count;
            }
        }
    }
}