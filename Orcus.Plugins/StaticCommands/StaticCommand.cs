using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Orcus.Plugins.PropertyGrid;

namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     A static command which executes an action without a session from the administration
    /// </summary>
    [Serializable, XmlInclude(typeof(ActiveStaticCommand))]
    public abstract class StaticCommand : IProvideEditableProperties
    {
        protected StaticCommand()
        {
            Properties = new List<IProperty>();
        }

        /// <summary>
        ///     The id of the command
        /// </summary>
        [XmlIgnore]
        public abstract Guid CommandId { get; }

        /// <summary>
        ///     The display name of the command
        /// </summary>
        [XmlIgnore]
        public abstract string Name { get; }

        /// <summary>
        ///     The description of the command
        /// </summary>
        [XmlIgnore]
        public abstract string Description { get; }

        /// <summary>
        ///     The category of the command
        /// </summary>
        [XmlIgnore]
        public abstract StaticCommandCategory Category { get; }

        /// <summary>
        ///     The registered properties
        /// </summary>
        [XmlIgnore]
        public virtual List<IProperty> Properties { get; }

        /// <summary>
        ///     Validate input
        /// </summary>
        /// <returns>Return false and display a message box if the user forgot an input</returns>
        public abstract InputValidationResult ValidateInput();

        /// <summary>
        ///     Get the command parameter
        /// </summary>
        public virtual CommandParameter GetCommandParameter()
        {
            return CommandParameter.FromProperties(this);
        }

        /// <summary>
        ///     Execute the command
        /// </summary>
        /// <param name="commandParameter">A command parameter transmitted by the administration</param>
        /// <param name="feedbackFactory">The feedback factory provides options to send log messages and finally return a result</param>
        /// <param name="clientInfo">Provides actions and information about the client</param>
        public abstract void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo);
    }
}