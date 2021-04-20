using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Administration.Extensions;

namespace Orcus.Test
{
    [TestClass]
    public class ViewUtilities
    {
        [TestMethod]
        public void TestWindowShowDialogProperty()
        {
            var window = new Window();
            Assert.IsFalse(window.IsModal());
        }
    }
}