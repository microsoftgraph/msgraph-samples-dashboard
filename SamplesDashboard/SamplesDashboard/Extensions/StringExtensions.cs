// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SamplesDashboard.Extensions
{
    public static class StringExtensions
    {
        private static char[] trimChars = { 'v', '=', '>', '<', '^', '~', ' ' };
        public static string NormalizeRequirementsString(this string requirements)
        {
            return requirements.TrimStart(trimChars).Trim();
        }
    }
}
