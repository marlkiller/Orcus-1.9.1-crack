using System;

namespace Orcus.Shared.DataTransferProtocol
{
    /// <summary>
    ///     Marks the method as a procedure/function for the <see cref="DtpProcessor" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DataTransferProtocolMethodAttribute : Attribute
    {
        public DataTransferProtocolMethodAttribute(params Type[] types)
        {
            ReturnTypes = types;
        }

        /// <summary>
        ///     Change the name of the method to a custom one
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        ///     Special types needed to serialize the return value
        /// </summary>
        public Type[] ReturnTypes { get; set; }
    }
}