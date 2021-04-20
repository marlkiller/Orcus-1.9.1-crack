using System;
using System.IO;
using System.Windows;

namespace Orcus.Administration.Utilities
{
    internal static class WpfUtilities
    {
        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            var resource = Application.GetResourceStream(new Uri(resourceName));

            using (var stream = resource.Stream)
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(file);
                }
            }
        }
    }
}