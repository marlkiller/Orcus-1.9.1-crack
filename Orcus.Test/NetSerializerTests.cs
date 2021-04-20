using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;

namespace Orcus.Test
{
    [TestClass]
    public class NetSerializerTests
    {
        [TestMethod, TestCategory("Serialization")]
        public void PrimitiveSerializeAndDeserialize()
        {
            var serializer = new Serializer(typeof (TestClass));
            var testObject = new TestClass
            {
                Property1 = "Garcon Un",
                Property2 = 34789594,
                Property3 = false,
                Property4 = 249059843253745
            };

            var data = serializer.Serialize(testObject);
            using (var testStream = new MemoryStream())
            {
                serializer.Serialize(testStream, testObject);
                Assert.IsTrue(data.SequenceEqual(testStream.ToArray()));

                var byteResult = serializer.Deserialize<TestClass>(data);

                testStream.Position = 0;
                var streamResult = (TestClass) serializer.Deserialize(testStream);

                Assert.IsNotNull(byteResult);
                Assert.IsNotNull(streamResult);

                Assert.AreEqual(byteResult.Property1, testObject.Property1);
                Assert.AreEqual(byteResult.Property2, testObject.Property2);
                Assert.AreEqual(byteResult.Property3, testObject.Property3);
                Assert.AreEqual(byteResult.Property4, testObject.Property4);

                Assert.AreEqual(streamResult.Property1, testObject.Property1);
                Assert.AreEqual(streamResult.Property2, testObject.Property2);
                Assert.AreEqual(streamResult.Property3, testObject.Property3);
                Assert.AreEqual(streamResult.Property4, testObject.Property4);
            }
        }

        [TestMethod, TestCategory("Serialization")]
        public void ComplexSerializeAndDeserialize()
        {
            var serializer =
                new Serializer(new[]
                {typeof (OnlineClientInformation), typeof (OfflineClientInformation), typeof (ClientInformation)});

            var testObject = new OnlineClientInformation
            {
                Id = 23490223,
                Group = "Garcon",
                Plugins =
                    new List<PluginInfo>
                    {
                        new PluginInfo
                        {
                            Guid = Guid.NewGuid(),
                            Name = "Mega Hyper Plugin 1.20903 Ultimate",
                            Version = "1.2.3"
                        }
                    },
                Port = 32894435,
                OnlineSince = DateTime.Now,
                OsType = OSType.Unknown
            };

            using (var testStream = new MemoryStream())
            {
                serializer.Serialize(testStream, testObject);
                testStream.Position = 0;
                var streamResult = (OnlineClientInformation) serializer.Deserialize(testStream);

                Assert.IsNotNull(streamResult);
                Assert.AreEqual(testObject.Id, streamResult.Id);
                Assert.AreEqual(testObject.Group, streamResult.Group);
                Assert.AreEqual(testObject.Plugins[0].Guid, streamResult.Plugins[0].Guid);
                Assert.AreEqual(testObject.Port, streamResult.Port);
                Assert.AreEqual(testObject.OsType, streamResult.OsType);
                Assert.AreEqual(testObject.OnlineSince, streamResult.OnlineSince);
            }
        }

        [Serializable]
        internal class TestClass
        {
            public string Property1 { get; set; }
            public int Property2 { get; set; }
            public bool Property3 { get; set; }
            public long Property4 { get; set; }
        }
    }
}