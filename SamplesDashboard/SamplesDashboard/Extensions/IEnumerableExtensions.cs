using System.Collections.Generic;
using System.Text;

namespace SamplesDashboard.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string BuildQueryString(this IEnumerable<string> names)
        {
            var flattenedString = new StringBuilder();
            foreach (var name in names)
            {
                flattenedString.Append(name + " OR ");
            }

            return flattenedString.Remove(flattenedString.Length - " OR ".Length, " OR ".Length).ToString();
        }
    }
}