using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Orcus.Administration.ViewModels.DataManager
{
    public class DataFilter
    {
        public List<int> ClientIds { get; set; }
        public string Type { get; set; }
        public string SearchText { get; set; }

        public bool IsAccepted(ViewData dataEntry)
        {
            if (ClientIds != null && !ClientIds.Contains(dataEntry.ClientId))
                return false;

            if (Type != null && dataEntry.DataManagerType.TypeId.IndexOf(Type, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            if (!string.IsNullOrEmpty(SearchText) &&
                (dataEntry.EntryName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) == -1 ||
                 dataEntry.DataManagerType.TypeId.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) == -1))
                return false;

            return true;
        }

        public static DataFilter ParseString(string s)
        {
            var result = new DataFilter();
            if (string.IsNullOrWhiteSpace(s))
                return result;

            var searchText = new StringBuilder();
            var parts = s.Split('"')
                .Select((element, index) => index % 2 == 0 // If even index
                    ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) // Split the item
                    : new[] { element }) // Keep the entire item
                .SelectMany(element => element).ToList();

            foreach (var part in parts)
            {
                if (Regex.IsMatch(part, "^CI-[0-9]{1,6}$", RegexOptions.IgnoreCase))
                {
                    var id = int.Parse(part.Substring(3));
                    if (result.ClientIds == null)
                        result.ClientIds = new List<int> { id };
                    else
                        result.ClientIds.Add(id);
                    continue;
                }

                var trueCondition = part.StartsWith("is:", StringComparison.OrdinalIgnoreCase);
                var falseCondition = part.StartsWith("isnot:", StringComparison.OrdinalIgnoreCase);

                if (trueCondition || falseCondition)
                {
                    result.Type = part.Substring(trueCondition ? 3 : 6, part.Length - (trueCondition ? 3 : 6));
                    continue;
                }

                searchText.Append(part);
            }

            result.SearchText = searchText.ToString();
            return result;
        }
    }
}