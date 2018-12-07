using NICE.Search.Common.Interfaces;
using NICE.Search.Common.Urls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Recommendations
{
    public static class SearchUrlExtensions
    {
            public static SearchUrl NoPage(this SearchUrl url)
            {
                return url
                    .Mod(s => s.pa = null)
                    .Mod(s => s.ps = null)
                    .Mod(s => s.om = null)
                    .Mod(s => s.cid = null)
                    .Mod(s => s.s = null);
            }

            public static SearchUrl Mod(this SearchUrl url, Action<SearchUrl> mod)
            {
                var copy = new SearchUrl(url);
                mod(copy);
                return copy;
            }

            public static ISearchUrl Mod(this ISearchUrl url, Action<ISearchUrl> mod)
            {
                var copy = new SearchUrl(url);
                mod(copy);
                return copy;
            }

            public static HtmlString Hiddens(this SearchUrl url)
            {
                var nonNullProperties = url.GetType()
                                           .GetProperties()
                                           .Where(p => p.CanRead)
                                           .Select(p => new { p.Name, value = p.GetValue(url) })
                                           .Where(p => p.value != null)
                                           .Where(p => p.Name != "fullUrl");

                var builder = new StringBuilder();
                foreach (var nonNullProperty in nonNullProperties)
                    builder.AppendFormat("<input name=\"{0}\" value='{1}' type=\"hidden\"/>", nonNullProperty.Name,
                                         nonNullProperty.value.ToString().Replace("'", "&#39;"));

                return new HtmlString(builder.ToString());
            }

            //om=[{"pty":["%20Patient%20Information%20"]},{"srn":["%20eMC%20"]}]
            public static T RemoveModifier<T>(
                this T url,
                Func<T, string> prop,
                string modifierName,
                Action<T, string> set)
            {
                var matchElements = new Regex(@"\{(.*?)}");
                var newList =
                    matchElements.Matches(prop(url))
                                 .Cast<Match>()
                                 .Select(m => m.Value)
                                 .Where(p => !p.Contains(modifierName))
                                 .ToList();

                if (!newList.Any())
                {
                    set(url, null);
                    return url;
                }
                set(url, "[" + string.Join(",", newList) + "]");

                return url;
            }

            public static T AddModifier<T>(this T url,
                Func<T, string> prop,
                string key,
                string value,
                Action<T, string> set)
            {
                var modifiers = url.ParseModifiers(prop);
                if (!modifiers.Any(m => m.Key == key && m.Value.Trim() == value))
                    modifiers.Add(new KeyValuePair<string, string>(key, value));

                set(url, "[" + string.Join(",", modifiers.Select(kvp => string.Format(@"{{""{0}"":["" {1} ""]}}", kvp.Key, kvp.Value.Trim()))) + "]");
                return url;
            }

            public static List<KeyValuePair<string, string>> ParseModifiers<T>(this T url, Func<T, string> prop)
            {
                var matchElements = new Regex(@"\{(.*?)}");
                var matchParts = new Regex(@"""([a-z]*?)"":\[""(.*?)""\]");
                var val = prop(url);
                if (val == null) return new List<KeyValuePair<string, string>>();
                return matchElements.Matches(val)
                                 .Cast<Match>()
                                 .Select(m => m.Groups[1].Value)
                                 .Select(p =>
                                 {
                                     var match = matchParts.Match(p);
                                     return new KeyValuePair<string, string>(match.Groups[1].Value, HttpUtility.UrlDecode(match.Groups[2].Value));
                                 })
                                 .ToList();
            }


            public static SearchUrl ToRecordsPerPage(this SearchUrl current, int amount)
            {
                var records = new SearchUrl(current);
                records.ps = amount;
                records.pa = null;
                return records;
            }

            public static SearchUrl ToRemoveDateRangeUrlModifier(this SearchUrl current)
            {
                var dateRange = new SearchUrl(current);
                dateRange.from = null;
                dateRange.to = null;
                return dateRange;
            }

            public static SearchUrl ToRemoveWithinPediodUrl(this SearchUrl current)
            {
                var dateRange = new SearchUrl(current);
                if (!string.IsNullOrEmpty(dateRange.om))
                    dateRange.RemoveModifier(s => s.om, "drm", (u, s) => { u.om = s; });
                return dateRange;
            }

            public static bool IsDateRangeSearch(this SearchUrl url)
            {
                return (!string.IsNullOrEmpty(url.from) && !string.IsNullOrEmpty(url.to));
            }

            public static IEnumerable<KeyValuePair<string, string>> EnumerateSearchUrlParameters(this SearchUrl current)
            {
                var fullUrl = current.fullUrl;
                if (!string.IsNullOrEmpty(fullUrl) && fullUrl.IndexOf('?') > -1)
                {
                    var query = System.Web.HttpUtility.ParseQueryString(fullUrl.Split('?')[1]);
                    foreach (var key in query.Keys)
                    {
                        yield return new KeyValuePair<string, string>(key.ToString(), query[key.ToString()]);
                    }
                }
                yield break;
            }

            public static void ValidateDatesForm(this SearchUrl current)
            {
                if (string.IsNullOrEmpty(current.from) && string.IsNullOrEmpty(current.to))
                    return;
                try
                {
                    DateTime.ParseExact(current.from, "dd/MM/yyyy", new DateTimeFormatInfo());
                }
                catch (Exception)
                {
                    throw new HttpException(400, "Bad from (date) format");
                }
                try
                {
                    DateTime.ParseExact(current.to, "dd/MM/yyyy", new DateTimeFormatInfo());
                }
                catch (Exception)
                {
                    throw new HttpException(400, "Bad to (date) format");
                }
            }

            public static SearchUrl RemoveRollupParams(this SearchUrl current)
            {
                var unrolledUrl = new SearchUrl(current);
                unrolledUrl.fc = null;
                unrolledUrl.cid = null;
                return unrolledUrl;
            }
        }
}