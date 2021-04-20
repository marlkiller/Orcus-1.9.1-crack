using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Shared.Utilities;

namespace Orcus.Test
{
    [TestClass]
    public class SharedTest
    {
        [TestMethod]
        public void TestGetFreeTempFileName()
        {
            var fi = new FileInfo(FileExtensions.GetFreeTempFileName());
            Assert.IsFalse(fi.Exists);

            fi = new FileInfo(FileExtensions.GetFreeTempFileName("mp3"));
            Assert.AreEqual(fi.Extension, ".mp3");
            Assert.IsFalse(fi.Exists);
        }

        [TestMethod]
        public void TestMakeUnique()
        {
            var fi = new FileInfo(FileExtensions.GetFreeTempFileName("mp4"));
            using (var writer = fi.OpenWrite())
                writer.WriteByte(12);

            try
            {
                var uniqueFile = new FileInfo(FileExtensions.MakeUnique(fi.FullName));
                Assert.IsFalse(uniqueFile.Exists);
                Assert.AreEqual(uniqueFile.Extension, ".mp4");
            }
            finally
            {
                fi.Delete();
            }
        }
    }
}