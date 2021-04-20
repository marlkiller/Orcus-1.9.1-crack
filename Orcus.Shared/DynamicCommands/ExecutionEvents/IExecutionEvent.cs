using System;

namespace Orcus.Shared.DynamicCommands.ExecutionEvents
{
    /// <summary>
    ///     Handler for an <see cref="ExecutionEvent" />
    /// </summary>
    public interface IExecutionEvent
    {
        /// <summary>
        ///     Return true if the execution event allows execution
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        ///     The id of the execution event
        /// </summary>
        uint Id { get; }

        /// <summary>
        ///     Raised when the execution event allows execution
        /// </summary>
        event EventHandler TheTimeHasCome;

        /// <summary>
        ///     Initialize the execution event
        /// </summary>
        /// <param name="parameter">
        ///     The parameter of the execution event provided by
        ///     <see cref="IExecutionEventBuilder.GetParameter" />
        /// </param>
        void Initialize(byte[] parameter);
    }
}