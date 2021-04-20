using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Test
{
    [TestClass]
    public class JpgCompressionTests
    {
        [TestMethod, TestCategory("Compression")]
        public void CompressionTest()
        {
            using (var jpg = new JpgCompression(100))
            {
                using (var bitmap = new Bitmap(200, 200))
                using (var memoryStream = new MemoryStream())
                {
                    jpg.Compress(bitmap, memoryStream);
                    Assert.IsTrue(memoryStream.Length > 0);
                }
            }
        }
    }
}