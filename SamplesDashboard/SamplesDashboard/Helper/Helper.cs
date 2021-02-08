using System.Collections.Generic;
using System.Text;

namespace SamplesDashboard.Helper
{
    public static class Helper
    {
        public static string BuildQueryString(IEnumerable<string> names)
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