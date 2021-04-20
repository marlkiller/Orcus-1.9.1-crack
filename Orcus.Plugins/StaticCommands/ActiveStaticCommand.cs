using System;

namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     A static command which executes an action with an undefined duration (e. g. looping) without a session from the
    ///     administration
    /// </summary>
    [Serializable]
    public abstract class ActiveStaticCommand : StaticCommand
    {
        /// <summary>
        ///     Automatically set to true, represents the current state of the command; will be set to false in
        ///     <see cref="StopExecute" />
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     This event should be fired when the command finished executing
        /// </summary>
        public event EventHandler ExecutionStopped;

        public sealed override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            IsActive = true;
            StartExecute(commandParameter, clientInfo);
        }

        /// <summary>
        ///     Execute the command. This method will run in an extra thread which can be blocked. Warning: if this method
        ///     finished, the command will still be considered running. Please fire the <see cref="ExecutionStopped" /> event when
        ///     the execution of the command is finished.
        /// </summary>
        /// <param name="commandParameter">A command parameter transmitted by the administration</param>
        /// <param name="clientInfo">Provides actions and information about the client</param>
        public abstract void StartExecute(CommandParameter commandParameter, IClientInfo clientInfo);

        /// <summary>
        ///     Stop the execution of the command
        /// </summary>
        public virtual void StopExecute()
        {
            IsActive = false;
            ExecutionStopped?.Invoke(this, EventArgs.Empty);
        }
    }
}