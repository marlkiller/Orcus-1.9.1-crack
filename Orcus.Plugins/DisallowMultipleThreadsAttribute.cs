using System;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Specify that the method which receives data packages can be executed only one time at once. This will also gurantee
    ///     that the packages will be received in the correct order; this attribute is very important if data packet are sent
    ///     which will be merged
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisallowMultipleThreadsAttribute : Attribute
    {
    }
}