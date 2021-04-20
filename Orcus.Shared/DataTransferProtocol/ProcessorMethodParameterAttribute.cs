using System;

namespace Orcus.Shared.DataTransferProtocol
{
    /// <summary>
    ///     Provides special types for a parameter of a method with the <see cref="DataTransferProtocolMethodAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ProcessorMethodParameterAttribute : Attribute
    {
        /// <summary>
        ///     Initialize a new instance of <see cref="ProcessorMethodParameterAttribute" />
        /// </summary>
        /// <param name="types">Special types needed to deserialize the parameter</param>
        public ProcessorMethodParameterAttribute(params Type[] types)
        {
            Types = types;
        }

        /// <summary>
        ///     Special types needed to deserialize the parameter
        /// </summary>
        public Type[] Types { get; }
    }
}