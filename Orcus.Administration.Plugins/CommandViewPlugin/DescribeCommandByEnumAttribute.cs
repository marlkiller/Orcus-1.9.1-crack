using System;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    public class DescribeCommandByEnumAttribute : Attribute
    {
        public DescribeCommandByEnumAttribute(Type enumType)
        {
            EumType = enumType;
        }

        public Type EumType { get; }
    }
}