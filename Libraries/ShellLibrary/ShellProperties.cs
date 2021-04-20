using System;
using System.Runtime.InteropServices;
using ShellLibrary.Native;

namespace ShellLibrary
{
    /// <summary>
    ///     Defines a partial class that implements helper methods for retrieving Shell properties
    ///     using a canonical name, property key, or a strongly-typed property. Also provides
    ///     access to all the strongly-typed system properties and default properties collections.
    /// </summary>
    public class ShellProperties : IDisposable
    {
        private ShellPropertyCollection defaultPropertyCollection;


        internal ShellProperties(ShellObject parent)
        {
            ParentShellObject = parent;
        }

        private ShellObject ParentShellObject { get; set; }

        /// <summary>
        ///     Gets the collection of all the default properties for this item.
        /// </summary>
        public ShellPropertyCollection DefaultPropertyCollection
        {
            get
            {
                if (defaultPropertyCollection == null)
                {
                    defaultPropertyCollection = new ShellPropertyCollection(ParentShellObject);
                }

                return defaultPropertyCollection;
            }
        }

        /// <summary>
        ///     Cleans up memory
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Returns a property available in the default property collection using
        ///     the given property key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty(PropertyKey key)
        {
            return CreateTypedProperty(key);
        }

        /// <summary>
        ///     Returns a property available in the default property collection using
        ///     the given canonical name.
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty(string canonicalName)
        {
            return CreateTypedProperty(canonicalName);
        }

        /// <summary>
        ///     Returns a strongly typed property available in the default property collection using
        ///     the given property key.
        /// </summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="key">The property key.</param>
        /// <returns>A strongly-typed ShellProperty for the given property key.</returns>
        public ShellProperty<T> GetProperty<T>(PropertyKey key)
        {
            return CreateTypedProperty(key) as ShellProperty<T>;
        }

        /// <summary>
        ///     Returns a strongly typed property available in the default property collection using
        ///     the given canonical name.
        /// </summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>A strongly-typed ShellProperty for the given canonical name.</returns>
        public ShellProperty<T> GetProperty<T>(string canonicalName)
        {
            return CreateTypedProperty(canonicalName) as ShellProperty<T>;
        }

        internal IShellProperty CreateTypedProperty<T>(PropertyKey propKey)
        {
            ShellPropertyDescription desc = ShellPropertyDescriptionsCache.Cache.GetPropertyDescription(propKey);
            return new ShellProperty<T>(propKey, desc, ParentShellObject);
        }

        internal IShellProperty CreateTypedProperty(PropertyKey propKey)
        {
            return ShellPropertyFactory.CreateShellProperty(propKey, ParentShellObject);
        }

        internal IShellProperty CreateTypedProperty(string canonicalName)
        {
            // Otherwise, call the native PropertyStore method
            PropertyKey propKey;

            int result = PropertySystemNativeMethods.PSGetPropertyKeyFromName(canonicalName, out propKey);

            if (!CoreErrorHelper.Succeeded(result))
            {
                throw new ArgumentException(
                    "The given CanonicalName is not valid.",
                    Marshal.GetExceptionForHR(result));
            }
            return CreateTypedProperty(propKey);
        }

        /// <summary>
        ///     Cleans up memory
        /// </summary>
        protected virtual void Dispose(bool disposed)
        {
            if (disposed && defaultPropertyCollection != null)
            {
                defaultPropertyCollection.Dispose();
            }
        }
    }
}