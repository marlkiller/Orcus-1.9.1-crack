using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Shared.Connection;
using Orcus.Shared.DataTransferProtocol;

namespace Orcus.Test
{
    [TestClass]
    public class DataTransferProtocolTest
    {
        private DtpFactory _dtpFactory;
        private DtpProcessor _dtpProcessor;

        [TestMethod, TestCategory("DataTransferProtocol")]
        public void TestDataTransferMethods()
        {
            _dtpFactory = new DtpFactory(SendData);
            _dtpProcessor = new DtpProcessor();

            _dtpProcessor.RegisterProcedure("TestMethod1",
                parameters => Trace.WriteLine("Parameterless method successfully executed"));
            _dtpProcessor.RegisterProcedure("TestMethod2", parameters =>
            {
                Assert.AreEqual(parameters.GetInt32(0), 19);
                Trace.WriteLine("Method with int parameter successfully executed");
            });
            _dtpProcessor.RegisterProcedure("TestMethod3",
                parameters =>
                {
                    var result = parameters.GetValue<List<ClientInformation>>(0, typeof (OnlineClientInformation),
                        typeof (OfflineClientInformation));
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Count, 3);
                    Assert.AreEqual(result[1].Group, "test");
                    Trace.WriteLine("Method with complex parameter successfully executed");
                });


            Trace.WriteLine("Executing parameterless method");
            _dtpFactory.ExecuteProcedure("TestMethod1");
            Trace.WriteLine("Executing method with in parameter");
            _dtpFactory.ExecuteProcedure("TestMethod2", 19);
            Trace.WriteLine("Executing method with complex parameter");
            _dtpFactory.ExecuteProcedure("TestMethod3",
                new List<Type> {typeof (OnlineClientInformation), typeof (OfflineClientInformation)}, null,
                new List<ClientInformation>
                {
                    new OnlineClientInformation(),
                    new OfflineClientInformation {Group = "test"},
                    new OnlineClientInformation()
                });
        }

        [TestMethod, TestCategory("DataTransferProtocol")]
        public void TestDataTransferFunctions()
        {
            _dtpFactory = new DtpFactory(SendData);
            _dtpProcessor = new DtpProcessor();

            _dtpProcessor.RegisterFunction("TestFunction1",
                parameters =>
                {
                    Trace.WriteLine("Parameterless function with int returning type successfully executed");
                    return 0;
                });

            _dtpProcessor.RegisterFunction("TestFunction2", parameters =>
            {
                Assert.AreEqual(parameters.GetInt32(0), 19);
                Trace.WriteLine("Function with int parameter and string returning type successfully executed");
                return "wtf";
            });

            _dtpProcessor.RegisterFunction("TestFunction3",
                parameters =>
                {
                    var result = parameters.GetValue<List<ClientInformation>>(0, typeof (OnlineClientInformation),
                        typeof (OfflineClientInformation));
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Count, 3);
                    Assert.AreEqual(result[1].Group, "test");
                    Trace.WriteLine("Function with complex parameter and complex returning successfully executed");
                    return new List<ClientInformation>
                    {
                        new OnlineClientInformation(),
                        new OfflineClientInformation(),
                        new OnlineClientInformation()
                    };
                }, typeof (OfflineClientInformation), typeof (OnlineClientInformation));


            Trace.WriteLine("Executing parameterless method");
            Assert.AreEqual(_dtpFactory.ExecuteFunction<int>("TestFunction1"), 0);

            Trace.WriteLine("Executing method with in parameter");
            Assert.AreEqual(_dtpFactory.ExecuteFunction<string>("TestFunction2", 19), "wtf");

            Trace.WriteLine("Executing method with complex parameter");
            Assert.AreEqual(_dtpFactory.ExecuteFunction<List<ClientInformation>>("TestFunction3",
                new List<Type> {typeof (OnlineClientInformation), typeof (OfflineClientInformation)},
                new List<Type> {typeof (OfflineClientInformation), typeof (OnlineClientInformation)},
                new List<ClientInformation>
                {
                    new OnlineClientInformation(),
                    new OfflineClientInformation {Group = "test"},
                    new OnlineClientInformation()
                }).Count, 3);
        }

        private void SendData(byte[] data)
        {
            _dtpFactory.Receive(_dtpProcessor.Receive(data));
        }
    }
}