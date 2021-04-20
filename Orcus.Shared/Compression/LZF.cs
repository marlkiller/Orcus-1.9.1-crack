/*
 * Remove the C++ things, make it static and add offset parameter:
 * Copyright (c) 2016 Alkalinee <https://github.com/Alkalinee>
 *
 * Improved version to C# LibLZF Port:
 * Copyright (c) 2010 Roman Atachiants <kelindar@gmail.com>
 * 
 * Original CLZF Port:
 * Copyright (c) 2005 Oren J. Maurice <oymaurice@hazorea.org.il>
 * 
 * Original LibLZF Library & Algorithm:
 * Copyright (c) 2000-2008 Marc Alexander Lehmann <schmorp@schmorp.de>
 * 
 * Redistribution and use in source and binary forms, with or without modifica-
 * tion, are permitted provided that the following conditions are met:
 * 
 *   1.  Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 * 
 *   2.  Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 * 
 *   3.  The name of the author may not be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MER-
 * CHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO
 * EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPE-
 * CIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTH-
 * ERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Alternatively, the contents of this file may be used under the terms of
 * the GNU General Public License version 2 (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of the above. If you wish to
 * allow the use of your version of this file only under the terms of the
 * GPL and not to allow others to use your version of this file under the
 * BSD license, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the GPL. If
 * you do not delete the provisions above, a recipient may use your version
 * of this file under either the BSD or the GPL.
 */

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Orcus.Shared.Compression
{
    /// <summary>
    ///     Improved C# LZF Compressor, a very small data compression library. The compression algorithm is extremely fast.
    /// </summary>
    public static class LZF
    {
        private const uint HLOG = 14;
        private const uint HSIZE = 1 << 14;
        private const uint MAX_LIT = 1 << 5;
        private const uint MAX_OFF = 1 << 13;
        private const uint MAX_REF = (1 << 8) + (1 << 3);
        private static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        public static extern unsafe IntPtr memcpy(void* dst, void* src, UIntPtr count);

        /// <summary>
        ///     Compresses the data using LibLZF algorithm
        /// </summary>
        /// <param name="input">Reference to the data to compress</param>
        /// <param name="output">Reference to a buffer which will contain the compressed data</param>
        /// <param name="start"></param>
        /// <returns>The size of the compressed archive in the output buffer</returns>
        private static int InternalCompress(byte[] input, byte[] output, int start)
        {
            var hashTable = new long[HSIZE];
            Array.Copy(BitConverter.GetBytes(input.Length - start), output, 4);

            uint iidx = (uint) start;
            uint oidx = 4;

            uint hval = (uint) ((input[iidx] << 8) | input[iidx + 1]); // FRST(in_data, iidx);
            int lit = 0;

            for (;;)
            {
                if (iidx < input.Length - 2)
                {
                    hval = (hval << 8) | input[iidx + 2];
                    long hslot = (hval ^ (hval << 5)) >> (int) (3*8 - HLOG - hval*5) & (HSIZE - 1);
                    var reference = hashTable[hslot];
                    hashTable[hslot] = iidx;


                    long off;
                    if ((off = iidx - reference - 1) < MAX_OFF
                        && iidx + 4 < input.Length
                        && reference > 0
                        && input[reference + 0] == input[iidx + 0]
                        && input[reference + 1] == input[iidx + 1]
                        && input[reference + 2] == input[iidx + 2]
                        )
                    {
                        /* match found at *reference++ */
                        uint len = 2;
                        uint maxlen = (uint) input.Length - iidx - len;
                        maxlen = maxlen > MAX_REF ? MAX_REF : maxlen;

                        if (oidx + lit + 1 + 3 >= output.Length)
                            return 0;

                        do
                            len++; while (len < maxlen && input[reference + len] == input[iidx + len]);

                        if (lit != 0)
                        {
                            output[oidx++] = (byte) (lit - 1);
                            lit = -lit;
                            do
                                output[oidx++] = input[iidx + lit]; while (++lit != 0);
                        }

                        len -= 2;
                        iidx++;

                        if (len < 7)
                        {
                            output[oidx++] = (byte) ((off >> 8) + (len << 5));
                        }
                        else
                        {
                            output[oidx++] = (byte) ((off >> 8) + (7 << 5));
                            output[oidx++] = (byte) (len - 7);
                        }

                        output[oidx++] = (byte) off;

                        iidx += len - 1;
                        hval = (uint) ((input[iidx] << 8) | input[iidx + 1]);

                        hval = (hval << 8) | input[iidx + 2];
                        hashTable[(hval ^ (hval << 5)) >> (int) (3*8 - HLOG - hval*5) & (HSIZE - 1)] = iidx;
                        iidx++;

                        hval = (hval << 8) | input[iidx + 2];
                        hashTable[(hval ^ (hval << 5)) >> (int) (3*8 - HLOG - hval*5) & (HSIZE - 1)] = iidx;
                        iidx++;
                        continue;
                    }
                }
                else if (iidx == input.Length)
                    break;

                /* one more literal byte we must copy */
                lit++;
                iidx++;

                if (lit == MAX_LIT)
                {
                    if (oidx + 1 + MAX_LIT >= output.Length)
                        return 0;

                    output[oidx++] = (byte) (MAX_LIT - 1);
                    lit = -lit;
                    do
                        output[oidx++] = input[iidx + lit]; while (++lit != 0);
                }
            }

            if (lit != 0)
            {
                if (oidx + lit + 1 >= output.Length)
                    return 0;

                output[oidx++] = (byte) (lit - 1);
                lit = -lit;
                do
                    output[oidx++] = input[iidx + lit]; while (++lit != 0);
            }

            return (int) oidx;
        }

        /// <summary>
        ///     Compresses the data using LibLZF algorithm
        /// </summary>
        /// <param name="source">Reference to the data to compress</param>
        /// <param name="start">The point where it should start to decompress from the <see cref="source" /></param>
        /// <returns>The compressed bytes</returns>
        public static unsafe byte[] Compress(byte[] source, int start)
        {
            var destination = new byte[source.Length + source.Length/2 + 36000];
            var used = InternalCompress(source, destination, start);
            if (used == 0)
                return null;

            var compressed = new byte[used];
            if (IsRunningOnMono)
            {
                Buffer.BlockCopy(destination, 0, compressed, 0, used);
            }
            else
            {
                fixed (byte* destinationPtr = compressed)
                fixed (byte* sourcePtr = destination)
                    memcpy(destinationPtr, sourcePtr, (UIntPtr) used);
            }

            return compressed;
        }

        /// <summary>
        ///     Compresses the data using LibLZF algorithm
        /// </summary>
        /// <param name="source">Reference to the data to compress</param>
        /// <param name="start">The point where it should start to decompress from the <see cref="source" /></param>
        /// <param name="lenght">The length of data in the return array</param>
        /// <returns>The compressed bytes</returns>
        public static byte[] Compress(byte[] source, int start, out int length)
        {
            length = 0;
            var destination = new byte[source.Length + source.Length/2 + 36000];
            var used = InternalCompress(source, destination, start);
            if (used == 0)
                return null;

            length = used;
            return destination;
        }

        /// <summary>
        ///     Decompresses the data using LibLZF algorithm
        /// </summary>
        /// <param name="source">Reference to the data to decompress</param>
        /// <param name="start">
        ///     The point where it should start to decompress from the <see cref="source"/</param>
        /// <returns>The decompressed bytes</returns>
        public static byte[] Decompress(byte[] source, int start)
        {
            var decompressed = new byte[GetSize(source, start)];
            var used = InternalDecompress(source, decompressed, start);
            if (used == 0)
                throw new InvalidOperationException("Impossible");

            return decompressed;
        }

        /// <summary>
        ///     Decompresses the data using LibLZF algorithm
        /// </summary>
        /// <param name="input">Reference to the data to decompress</param>
        /// <param name="output">Reference to a buffer which will contain the decompressed data</param>
        /// <param name="start"></param>
        /// <returns>Returns decompressed size</returns>
        private static int InternalDecompress(byte[] input, byte[] output, int start)
        {
            uint iidx = 4 + (uint) start;
            uint oidx = 0;

            do
            {
                uint ctrl = input[iidx++];

                if (ctrl < 1 << 5) /* literal run */
                {
                    ctrl++;

                    if (oidx + ctrl > output.Length)
                    {
                        //SET_ERRNO (E2BIG);
                        return 0;
                    }

                    do
                        output[oidx++] = input[iidx++]; while (--ctrl != 0);
                }
                else /* back reference */
                {
                    uint len = ctrl >> 5;

                    int reference = (int) (oidx - ((ctrl & 0x1f) << 8) - 1);

                    if (len == 7)
                        len += input[iidx++];

                    reference -= input[iidx++];

                    if (oidx + len + 2 > output.Length)
                    {
                        //SET_ERRNO (E2BIG);
                        return 0;
                    }

                    if (reference < 0)
                    {
                        //SET_ERRNO (EINVAL);
                        return 0;
                    }

                    output[oidx++] = output[reference++];
                    output[oidx++] = output[reference++];

                    do
                        output[oidx++] = output[reference++]; while (--len != 0);
                }
            } while (iidx < input.Length);

            return (int) oidx;
        }

        private static int GetSize(byte[] source, int start)
        {
            return BitConverter.ToInt32(source, start);
        }
    }
}