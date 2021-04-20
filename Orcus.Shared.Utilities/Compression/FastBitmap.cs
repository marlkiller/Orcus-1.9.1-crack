using System;
using System.Drawing.Imaging;

/*
    The MIT License (MIT)
    Copyright (c) 2016 AnguisCaptor
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace Orcus.Shared.Utilities.Compression
{
    internal class FastBitmap
    {
        public static int CalcImageOffset(int x, int y, PixelFormat format, int width)
        {
            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                    return y*width*4 + x*4;
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return y*width*3 + x*3;
                case PixelFormat.Format8bppIndexed:
                    return y*width + x;
                case PixelFormat.Format4bppIndexed:
                    return y*(width/2) + x/2;
                case PixelFormat.Format1bppIndexed:
                    return y*width*8 + x*8;
                default:
                    throw new NotSupportedException(format + " is not supported.");
            }
        }
    }
}