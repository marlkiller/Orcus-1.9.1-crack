using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.NetSerializer;

namespace ConsoleApplication1
{
    class Program
    {
		//Result on my Computer:
		//2081
		//1829
        static void Main(string[] args)
        {
            var test = new Class2();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var parameters =
                    new DtpParameters(new Dictionary<int, byte[]> {{0, new Serializer(typeof (int)).Serialize(0x13)}});

                foreach (var methodInfo in test.GetType().GetMethods())
                {
                    var attribute =
                        methodInfo.GetCustomAttributes(false).OfType<ProcessorMethodAttribute>().FirstOrDefault();
                    if (attribute == null)
                        continue;

                    var methodName = attribute.MethodName ?? methodInfo.Name;

                    if (methodInfo.ReturnType == typeof(void))
                        methodInfo.Invoke(test, GetParameters(parameters, methodInfo));
                    else
                        methodInfo.Invoke(test, GetParameters(parameters, methodInfo));
                }
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var parameters =
                    new DtpParameters(new Dictionary<int, byte[]> {{0, new Serializer(typeof (int)).Serialize(0x13)}});
                test.Test(parameters.GetInt32(0));
                test.Test2(parameters.GetInt32(0));
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();

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
    }
	
    public class Class2
    {
        [ProcessorMethod]
        public void Test(int test)
        {
        }

        [ProcessorMethod]
        public string Test2(int test)
        {
            return "asd";
        }
    }
}
