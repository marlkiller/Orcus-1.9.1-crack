using System;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     An exception occurred which depends on the plugin system of Orcus
    /// </summary>
    public class PluginException : Exception
    {
        /// <summary>
        ///     Create a new instance of <see cref="PluginException" />
        /// </summary>
        /// <param name="logMessage">Define a message which will be added to the administration log</param>
        public PluginException(string logMessage)
        {
            LogMessage = logMessage;
        }

        /// <summary>
        ///     The message which should be added to the log
        /// </summary>
        public string LogMessage { get; }
    }
}