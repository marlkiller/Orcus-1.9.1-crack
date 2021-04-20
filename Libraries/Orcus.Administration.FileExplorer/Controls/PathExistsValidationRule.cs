using System;
using System.Globalization;
using System.Windows.Controls;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.FileExplorer.Controls
{
    public class PathExistsValidationRule : ValidationRule
    {
        private readonly IHierarchyHelper _hierarchyHelper;
        private readonly object _root;

        public PathExistsValidationRule(IHierarchyHelper hierarchyHelper, object root)
        {
            _hierarchyHelper = hierarchyHelper;
            _root = root;
        }

        public PathExistsValidationRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (!(value is string))
                    return new ValidationResult(false, "Invalid Path");

                if (_hierarchyHelper.GetItem(_root, (string) value) == null)
                    return new ValidationResult(false, "Path Not Found");
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Invalid Path");
            }
            return new ValidationResult(true, null);
        }
    }
}