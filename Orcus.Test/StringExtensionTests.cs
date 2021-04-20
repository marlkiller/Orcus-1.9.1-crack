using System;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Administration.Core.Utilities;
using FileExtensions = Orcus.Shared.Utilities.FileExtensions;

namespace Orcus.Test
{
    //Source: https://gist.github.com/Galilyou/00dcd0dab2d2a050c30c
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public string ReplaceCalledOnNullOrEmptyReturnsNullOrEmpty(string src, string oldValue, string newValue)
        {
            var result = src.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);

            return result;
        }

        [TestMethod]
        public void ReplaceGivenVariedCaseStringReplacesCorrectly()
        {
            string s = "Hello everyone from {{RecipIenT}}";

            var result = s.Replace("{{recipient}}", "John Doe", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(result, "Hello everyone from John Doe");
        }

        [TestMethod]
        public void ReplaceGivenVariedCaseReplacesAllOccurences()
        {
            string s =
                "Hello everyone from {{RecipIenT}}. Allow me, {{reCIPIeNt}} to welcome you all to {{RECIpieNT}}'s party.";

            var result = s.Replace("{{recipient}}", "John Doe", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(result,
                "Hello everyone from John Doe. Allow me, John Doe to welcome you all to John Doe's party.");
        }

        [TestMethod]
        public void ReplaceGivenNonExistingTermReturnsOriginalString()
        {
            string s = "This string doesn't inlcude the term to be removed";
            var result = s.Replace("not-here", "whatever", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(s, result);
        }

        [TestMethod]
        public void ReplaceWhenOldValueAreEqualNewValue()
        {
            string s = "replace my OldValue here please. OldValue dafds fOldValue";
            var result = s.Replace("OldValue", "OldValue", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(s, result);
        }

        [TestMethod]
        public void ReplaceEmptyStringWithEmptyString()
        {
            string s = "This is a string";
            string result = s.Replace("", "", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(result, s);
        }

        [TestMethod]
        public void ReplaceWhenOldValueLenghtIsGreaterThanSourceReturnsSource()
        {
            string src = "œ";
            string result = src.Replace("oe", "", StringComparison.InvariantCulture);

            Assert.AreEqual(result, src);
        }

        // we only really care for english, but Arabic is the only other potential possibility
        [TestMethod]
        public void ReplaceWorksWithArabic()
        {
            var src = "الشعر في العربية شئ جميل شئ جميل ان تكتب شعر في العربية، لان شعر العربية شئ جميل";
            var result = src.Replace("شعر", "نثر", StringComparison.OrdinalIgnoreCase);

            Assert.AreEqual(result, "النثر في العربية شئ جميل شئ جميل ان تكتب نثر في العربية، لان نثر العربية شئ جميل");
        }

        [TestMethod]
        public void SecureStringToString()
        {
            var sampleString = "test123";
            var secureString = new SecureString();
            foreach (var c in sampleString)
                secureString.AppendChar(c);

            Assert.AreEqual(secureString.Length, sampleString.Length);
            var result = StringExtensions.SecureStringToString(secureString);
            Assert.AreEqual(sampleString, result);
        }

        [TestMethod]
        public void NormalizePathTest()
        {
            var path1 = "C:\\Users\\asd\\AppData\\Roaming\\Skype\\araw";
            var path2 = "C:\\Users\\asd\\AppData\\Roaming\\Skype\\ARAW";

            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path2 = "C:\\Users\\asd\\AppData\\Roaming\\Skype\\ARAW\\";

            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path2 = "C:\\Users\\asd\\AppData\\Roaming\\test\\..\\Skype\\ARAW\\";
            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path2 = ":ASD?§)=~asd#'=()\\\\\\\\asd21//{}?";
            Assert.AreNotEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path1 = path2;
            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path1 = "I'm crazy\\\\//";
            path2 = path1;

            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path2 = "asda";
            Assert.AreNotEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path1 = "C:\\";
            path2 = "C:\\";

            Assert.AreEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));

            path2 = "C:\\test";
            Assert.AreNotEqual(FileExtensions.NormalizePath(path1),
                FileExtensions.NormalizePath(path2));
        }
    }
}