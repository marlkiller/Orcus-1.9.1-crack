using System;
using System.Runtime.InteropServices;

namespace OpusWrapper.Native
{
    /// <summary>
    ///     Wraps the Opus API.
    /// </summary>
    internal class OpusApi
    {
        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void opus_encoder_destroy(IntPtr encoder);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int opus_encode(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe int opus_encode(IntPtr st, byte* pcm, int frame_size, IntPtr data, int max_data_bytes);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr opus_decoder_create(int Fs, int channels, out IntPtr error);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void opus_decoder_destroy(IntPtr decoder);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int opus_decode(IntPtr st, byte[] data, int len, IntPtr pcm, int frame_size,
            int decode_fec);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe int opus_decode(IntPtr st, byte* data, int len, IntPtr pcm, int frame_size,
            int decode_fec);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int opus_encoder_ctl(IntPtr st, Ctl request, int value);

        [DllImport("opus.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int opus_encoder_ctl(IntPtr st, Ctl request, out int value);
    }
}