using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.MP3;
using CSCore.SoundOut;
using CSCore.Streams;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.AudioPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.Audio
{
    public class AudioPlayer : PropertyChangedBase, IDisposable
    {
        private Stream _currentMemoryStream;
        private SimpleNotificationSource _simpleNotificationSource;
        private ISoundOut _soundOut;
        private IWaveSource _soundSource;
        private CancellationTokenSource _soundSourcePositionToken;
        private long _trackPosition;
        private TimeSpan _trackPositionTime;
        private float _volume = 0.75f;

        public void Dispose()
        {
            StopPlayback();
            _soundOut?.Dispose();
            _soundSource?.Dispose();
            _simpleNotificationSource?.Dispose();
            _currentMemoryStream?.Dispose();
        }

        public long TrackLength => _soundSource?.Length ?? 0;
        public TimeSpan TrackLengthTime => _soundSource?.GetLength() ?? TimeSpan.Zero;
        public bool IsPlaying => _soundOut != null && _soundOut.PlaybackState == PlaybackState.Playing;

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(value, ref _volume) && _soundOut != null)
                    _soundOut.Volume = value;
            }
        }

        public TimeSpan TrackPositionTime
        {
            get { return _soundSource == null ? TimeSpan.Zero : _trackPositionTime; }
            private set
            {
                if ((int) value.TotalSeconds != (int) _trackPositionTime.TotalSeconds)
                    SetProperty(value, ref _trackPositionTime);
            }
        }

        public long TrackPosition
        {
            get { return _soundSource == null ? 0 : _trackPosition; }
            set
            {
                if (_soundSource != null)
                {
                    _soundSourcePositionToken?.Cancel();
                    _soundSourcePositionToken = new CancellationTokenSource();
                    SetSoundSourcePosition(value, _soundSourcePositionToken.Token).Forget();
                    _trackPosition = value;
                    OnPositionChanged();
                }
            }
        }

        public void Open(IAudioFile file)
        {
            StopPlayback();
            _soundSource?.Dispose();
            _simpleNotificationSource?.Dispose();

            _currentMemoryStream = new MemoryStream(file.Data);
            _soundSource =
                new DmoMp3Decoder(_currentMemoryStream).AppendSource(
                    x => new SimpleNotificationSource(x.ToSampleSource()),
                    out _simpleNotificationSource).ToWaveSource();
            _simpleNotificationSource.BlockRead += SimpleNotificationSource_BlockRead;

            if (_soundOut == null)
            {
                _soundOut = new WasapiOut();
                _soundOut.Stopped += SoundOut_Stopped;
            }

            _soundOut.Initialize(_soundSource);
            _soundOut.Volume = Volume;

            OnTrackLengthChanged();
            TrackPosition = 0;
            OnPositionChanged();
        }

        public void TooglePlayPause()
        {
            if (_soundOut?.PlaybackState == PlaybackState.Playing)
            {
                _soundOut?.Pause();
            }
            else
            {
                _soundOut?.Play();
            }
            CurrentStateChanged();
        }

        private void StopPlayback()
        {
            if (_soundOut != null &&
                (_soundOut.PlaybackState == PlaybackState.Playing || _soundOut.PlaybackState == PlaybackState.Paused))
            {
                _soundOut.Stop();
                CurrentStateChanged();
            }
        }

        private void SimpleNotificationSource_BlockRead(object sender, EventArgs e)
        {
            _trackPosition = _soundSource.Position;
            OnPositionChanged();
        }

        protected void OnPositionChanged()
        {
            if (_soundSource == null)
                return;
            TrackPositionTime = TimeSpan.FromMilliseconds(_soundSource.WaveFormat.BytesToMilliseconds(TrackPosition));
            OnPropertyChanged(nameof(TrackPosition));
        }

        private async Task SetSoundSourcePosition(long value, CancellationToken token)
        {
            try
            {
                await Task.Run(() => _soundSource.Position = value, token);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SoundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            TrackPosition = 0;
            OnPropertyChanged(nameof(IsPlaying));
        }

        protected void CurrentStateChanged()
        {
            OnPropertyChanged(nameof(IsPlaying));
        }

        protected void OnTrackLengthChanged()
        {
            OnPropertyChanged(nameof(TrackLength));
            OnPropertyChanged(nameof(TrackLengthTime));
        }
    }
}