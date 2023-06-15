using System.Globalization;
using System.Text.RegularExpressions;

namespace Blog.Helpers
{
    public class StringHelper
    {
        public static string BlogPostSlug(string? title)
        {
            // Remove all accents and make string lower case
            string? output = RemoveAccents(title).ToLower();

            // Remove special characters
            output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");

            // Remove all additional spaces in favour of just one.
            output = Regex.Replace(output, @"\s+", " ");

            // Replace all spaces with the hyphen
            output = Regex.Replace(output, @"\s", "-");

            return output;
        }
        private static string RemoveAccents(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return title!;
            }

            // Convert for Unicode
            title = title.Normalize(System.Text.NormalizationForm.FormD);
            // Format unicode/ascii
            char[] chars = title.Where(c=> CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            // Convert and return the new title
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);

        }
    }
}
