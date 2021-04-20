using System;

namespace Orcus.Shared.Core
{
    /// <summary>
    ///     Serialize the date time as UTC and convert it to local time after deserialisation. Makes the date time timezone
    ///     independent
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializeAsUtcAttribute : Attribute
    {
    }
}