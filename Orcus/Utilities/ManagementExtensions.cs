using System;
using System.Management;

namespace Orcus.Utilities
{
    public static class ManagementExtensions
    {
        public static T TryGetProperty<T>(this ManagementObject managementObject, string propertyName)
        {
            try
            {
                return (T) managementObject[propertyName];
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static DateTime? ToDateTimeSafe(string dmtfDate)
        {
            try
            {
                return ManagementDateTimeConverter.ToDateTime(dmtfDate);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}