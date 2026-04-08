using System.Text;
using System.Text.RegularExpressions;

namespace Itero.API
{
    public static class StringExtension
    {
        public static string Capitalize(this string str)
        {
            char[] letters = str.ToLower().ToCharArray();
            letters[0] = char.ToUpper(letters[0]);

            return string.Join("", letters);
        }

        public static string RemoveMultispaces(this string str)
        {
            str = str.Trim();
            str = Regex.Replace(str, @"\s+", " ");

            return new string(str);
        }

        public static string WrapWithBracketsIfNeeded(this string str)
        {
            if (!str.StartsWith("["))
                str = "[" + str;
            if (!str.EndsWith("]"))
                str = str + "]";

            return str;
        }

        public static string AddPointIfNeeded(this string str)
        {
            if (!str.TrimEnd().EndsWith('.'))
                str = str + '.';

            return str;
        }


        public static float GetSimilarity(this string str1, string str2)
        {
            str1 = str1.ToLower().Replace(" ", "");
            str2 = str2.ToLower().Replace(" ", "");

            char[] str1_array = str1.ToCharArray();
            char[] str2_array = str2.ToCharArray();

            char[] unique_letters = str1_array.Union(str2_array).ToArray();

            char[] intersect = str1_array.Intersect(str2_array).ToArray();
            int sub_intersect = Math.Abs(intersect.Length - unique_letters.Length);

            return intersect.Length / (unique_letters.Length + sub_intersect * 0.5f);
        }


        public static string[] SplitIgnored(this string str, char separator, char ignore)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool isQuotes = false;

            foreach (char ch in str.RemoveMultispaces())
            {
                if (ch == ignore)
                {
                    isQuotes = !isQuotes;
                    continue;
                }

                if (ch == separator && isQuotes == false)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            parts.Add(current.ToString());

            return parts.ToArray();
        }
    }
}
