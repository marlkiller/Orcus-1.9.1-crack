using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Shared.Encryption;

namespace Orcus.Test
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    internal class AESTests
    {
        [TestMethod, TestCategory("Encryption")]
        public void EncryptAndDecryptStringTest()
        {
            var password = Guid.NewGuid().ToString("N");
            var testString = Guid.NewGuid().ToString("N");

            var encrypted = AES.Encrypt(testString, password);
            Assert.AreNotEqual(testString, encrypted);
            Assert.AreNotEqual(password, encrypted);

            var decrypted = AES.Decrypt(encrypted, password);
            Assert.AreEqual(testString, decrypted);
        }
    }
}