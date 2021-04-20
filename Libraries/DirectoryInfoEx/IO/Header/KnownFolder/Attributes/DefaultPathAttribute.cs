using System;

namespace ShellDll
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DefaultPathAttribute : Attribute
    {
        public string DefaultPath { get; set; }

        public DefaultPathAttribute(string defaultPath)
        {
            DefaultPath = defaultPath;
        }
    }
}
