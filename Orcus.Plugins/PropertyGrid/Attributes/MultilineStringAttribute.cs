using System;

namespace Orcus.Plugins.PropertyGrid.Attributes
{
    /// <summary>
    ///     The string can be multiline, a bigger text field will appear and returns will be accepted
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MultilineStringAttribute : Attribute
    {
    }
}