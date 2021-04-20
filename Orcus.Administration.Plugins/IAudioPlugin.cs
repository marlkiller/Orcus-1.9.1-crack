using System.Collections.Generic;
using Orcus.Administration.Plugins.AudioPlugin;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     A plugin which provides new sounds for the audio command
    /// </summary>
    public interface IAudioPlugin
    {
        /// <summary>
        ///     This function returns the audio files provided by the plugin
        /// </summary>
        IEnumerable<IAudioFile> AudioFiles { get; }
    }
}