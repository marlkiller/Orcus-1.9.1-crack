using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Orcus.Administration.Commands.Native;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.Commands.FileExplorer
{
    public static class ShellPropertyHelper
    {
        public static string GetDisplayName(this ShellProperty shellProperty)
        {
            var propertyKey = new PropertyKey(shellProperty.FormatId, shellProperty.PropertyId);
            Guid guid = new Guid("6F79D558-3E96-4549-A1D1-7D75D2288814");
            IPropertyDescription nativePropertyDescription;

            NativeMethods.PSGetPropertyDescription(ref propertyKey, ref guid,
                out nativePropertyDescription);

            if (nativePropertyDescription != null)
                try
                {
                    IntPtr dispNamePtr;

                    int hr = nativePropertyDescription.GetDisplayName(out dispNamePtr);

                    if (hr >= 0 && dispNamePtr != IntPtr.Zero)
                    {
                        var name = Marshal.PtrToStringUni(dispNamePtr);

                        // Free the string
                        Marshal.FreeCoTaskMem(dispNamePtr);

                        return name;
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(nativePropertyDescription);
                }

            return FormatString(shellProperty.Name);
        }

        private static string FormatString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString))
                return null;

            var lastPartIndex = rawString.LastIndexOf('.');
            return Regex.Replace((lastPartIndex > -1 ? rawString.Substring(lastPartIndex + 1) : rawString),
                @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
        }
    }
}