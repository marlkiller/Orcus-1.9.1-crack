using System;
using System.Windows.Media.Imaging;

namespace Orcus.Administration.Plugins.AudioPlugin
{
    /// <summary>
    ///     Represents an audio file
    /// </summary>
    public interface IAudioFile : IDisposable
    {
        /// <summary>
        ///     The data of the audio file
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        ///     The duration of the audio file
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     The genre of the audio file
        /// </summary>
        AudioGenre Genre { get; }

        /// <summary>
        ///     The name of the audio file
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     A small thumbnail for the audio file. If null, a picture of the wave form is shown
        /// </summary>
        BitmapImage Thumbnail { get; }
    }
}