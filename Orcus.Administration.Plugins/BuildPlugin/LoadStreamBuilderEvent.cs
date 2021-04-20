using System.IO;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Modify the source of Orcus. Only one plugin can subscribe to this event in the building process
    /// </summary>
    public class LoadStreamBuilderEvent : BuilderEvent
    {
        /// <summary>
        ///     Get the source of a client (assembly)
        /// </summary>
        /// <param name="builderInformation">The current builder settings</param>
        /// <returns>A stream which results in an assembly</returns>
        public delegate Stream LoadStreamDelegate(IBuilderInformation builderInformation);

        /// <summary>
        ///     Initialize a new instance of <see cref="LoadStreamBuilderEvent" />
        /// </summary>
        /// <param name="loadStreamDelegate">The delegate to execute</param>
        public LoadStreamBuilderEvent(LoadStreamDelegate loadStreamDelegate)
        {
            LoadStream = loadStreamDelegate;
        }

        public LoadStreamDelegate LoadStream { get; }
    }
}