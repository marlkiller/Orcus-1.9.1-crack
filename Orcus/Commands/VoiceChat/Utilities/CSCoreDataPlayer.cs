using System;
using CSCore;
using CSCore.SoundOut;
using CSCore.Streams;
using OpusWrapper;

namespace Orcus.Commands.VoiceChat.Utilities
{
    // ReSharper disable once InconsistentNaming
    public class CSCoreDataPlayer : IDisposable
    {
        private readonly bool _triggerSingleBlockRead;
        private WasapiOut _wasapiOut;
        private WriteableBufferingSource _writeableBufferingSource;
        private OpusDecoder _opusDecoder;
        private readonly object _componentsLock = new object();
        private bool _isDisposed;

        public CSCoreDataPlayer(bool triggerSingleBlockRead)
        {
            _triggerSingleBlockRead = triggerSingleBlockRead;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            lock (_componentsLock)
            {
                _isDisposed = true;

                _wasapiOut.Dispose();
                _opusDecoder.Dispose();
                _writeableBufferingSource.Dispose();
            }
        }

        public event EventHandler<SingleBlockReadEventArgs> SingleBlockRead;

        public void Initialize()
        {
            _wasapiOut = new WasapiOut();
            _opusDecoder = OpusDecoder.Create(48000, 1);

            //var waveForm = new WaveFormatExtensible(48000, 16, 1, Guid.Parse("00000003-0000-0010-8000-00aa00389b71"));
            var waveForm = new WaveFormat(48000, 16, 1);

            _writeableBufferingSource = new WriteableBufferingSource(waveForm) {FillWithZeros = true};

            IWaveSource waveSource;
            if (_triggerSingleBlockRead)
            {
                var singleBlockNotificationStream =
                    new SingleBlockNotificationStream(_writeableBufferingSource.ToSampleSource());
                singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStreamOnSingleBlockRead;
                waveSource = singleBlockNotificationStream.ToWaveSource();
            }
            else
                waveSource = _writeableBufferingSource;
           
            _wasapiOut.Initialize(waveSource);
            _wasapiOut.Play();
        }

        private void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs singleBlockReadEventArgs)
        {
            SingleBlockRead?.Invoke(this, singleBlockReadEventArgs);
        }

        public unsafe void Feed(byte[] bytes, int offset, int count)
        {
            if (_isDisposed)
                return;

            lock (_componentsLock)
            {
                if (_isDisposed)
                    return;

                var position = offset;
                fixed (byte* bytesPtr = bytes)
                    while (position != count + offset)
                    {
                        var segmentLength = BitConverter.ToInt32(bytes, position);
                        position += 4;

                        int bufferLength;
                        var decodedBuffer = _opusDecoder.Decode(bytesPtr + position, segmentLength, out bufferLength);

                        _writeableBufferingSource.Write(decodedBuffer, 0, bufferLength);
                        position += segmentLength;
                    }
            }
        }
    }
}