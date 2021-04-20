using System;

namespace ShellDll
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CsidlAttribute : Attribute
    {
        public ShellAPI.CSIDL CSIDL { get; set; }        

        public CsidlAttribute(ShellAPI.CSIDL csidl)
        {
            CSIDL = csidl;
        }
    }
}
