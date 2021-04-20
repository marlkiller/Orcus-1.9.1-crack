using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orcus.Administration.FileExplorer.Models
{
    /// <summary>
    ///     Suggest based on sub entries of specified data.
    /// </summary>
    public class AutoSuggestSource : ISuggestSource
    {
        public Task<IList<object>> SuggestAsync(object data, string input, IHierarchyHelper helper)
        {
            if (helper == null)
                return Task.FromResult<IList<object>>(new List<Object>());

            string valuePath = helper.ExtractPath(input);
            string valueName = helper.ExtractName(input);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(helper.Separator + ""))
                valueName += helper.Separator;

            if (valuePath == "" && input.EndsWith("" + helper.Separator))
                valuePath = valueName;
            var found = helper.GetItem(data, valuePath);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                var enuma =
                helper.List(found);
                foreach (var item in enuma)
                {
                    string valuePathName = helper.GetPath(item);
                    if (valuePathName.StartsWith(input, helper.StringComparisonOption) &&
                        !valuePathName.Equals(input, helper.StringComparisonOption))
                        retVal.Add(item);
                }
            }

            return Task.FromResult<IList<object>>(retVal);
        }
    }
}