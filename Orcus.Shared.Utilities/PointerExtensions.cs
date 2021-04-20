using System;
using System.Runtime.ConstrainedExecution;

namespace Orcus.Shared.Utilities
{
    static class PointerExtensions
    {
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static IntPtr Add(this IntPtr pointer, int offset)
        {
            unchecked
            {
                switch (IntPtr.Size)
                {
                    case sizeof (Int32):
                        return (new IntPtr(pointer.ToInt32() + offset));
                    default:
                        return (new IntPtr(pointer.ToInt64() + offset));
                }
            }
        }
    }
}