using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.FileExplorer.Controls
{
    /// <summary>
    ///     Use Path to query for hierarchy of ViewModels.
    /// </summary>
    public class PathHierarchyHelper : IHierarchyHelper
    {
        public PathHierarchyHelper(string parentPath, string valuePath, string subEntriesPath)
        {
            ParentPath = parentPath;
            ValuePath = valuePath;
            SubentriesPath = subEntriesPath;
            Separator = '\\';
            StringComparisonOption = StringComparison.CurrentCultureIgnoreCase;
        }

        public virtual string ExtractPath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }

        public virtual string ExtractName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            if (pathName.EndsWith(":\\") && pathName.Length == 3)
                return pathName.Substring(0, 2);

            return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }

        public IEnumerable<object> GetHierarchy(object item, bool includeCurrent)
        {
            if (includeCurrent)
                yield return item;

            var current = getParent(item);
            while (current != null)
            {
                yield return current;
                current = getParent(current);
            }
        }

        public string GetPath(object item)
        {
            return item == null ? "" : getValuePath(item);
        }

        public object GetItem(object rootItem, string path)
        {
            var queue = new Queue<string>(path.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries));
            object current = rootItem;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                foreach (var item in List(current))
                {
                    string valuePathName = getValuePath(item);
                    string value = ExtractName(valuePathName); //Value may be full path, or just current value.
                    if (value.Equals(nextSegment, StringComparisonOption))
                    {
                        found = item;
                        break;
                    }
                }
                current = found;
            }
            return current;
        }

        public char Separator { get; set; }
        public StringComparison StringComparisonOption { get; set; }
        public string ParentPath { get; set; }
        public string ValuePath { get; set; }
        public string SubentriesPath { get; set; }

        protected virtual object getParent(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, ParentPath);
        }

        protected virtual string getValuePath(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, ValuePath) as string;
        }

        protected virtual IEnumerable getSubEntries(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, SubentriesPath) as IEnumerable;
        }

        public IEnumerable List(object item)
        {
            return item is IEnumerable ? item as IEnumerable : getSubEntries(item);
        }
    }

    /// <summary>
    ///     Generic version of AutoHierarchyHelper, which use Path to query for hierarchy of ViewModels
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PathHierarchyHelper<T> : PathHierarchyHelper
    {
        PropertyInfo propInfoValue, propInfoSubEntries, propInfoParent;

        public PathHierarchyHelper(string parentPath, string valuePath, string subEntriesPath)
            : base(parentPath, valuePath, subEntriesPath)
        {
            propInfoSubEntries = typeof (T).GetProperty(subEntriesPath);
            propInfoValue = typeof (T).GetProperty(valuePath);
            propInfoParent = typeof (T).GetProperty(parentPath);
        }

        protected override object getParent(object item)
        {
            return propInfoParent.GetValue(item);
        }

        protected override IEnumerable getSubEntries(object item)
        {
            return propInfoSubEntries.GetValue(item) as IEnumerable;
        }

        protected override string getValuePath(object item)
        {
            return propInfoValue.GetValue(item) as string;
        }
    }
}