using System.Text;
using System.Text.RegularExpressions;

namespace Itereta.Common
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

        public static string AddLastPointIfNeeded(this string str)
        {
            if (!str.TrimEnd().EndsWith('.'))
                str = str + '.';

            return str;
        }


        public static double ComputeLevenshteinSimilarity(this string a, string b)
        {
            const double delete_cost = 1.0d;
            const double insertion_cost = 1.1d;
            const double replacement_cost = 1.05d;

            a = a.ToLower().Replace(" ", "");
            b = b.ToLower().Replace(" ", "");

            int m = a.Length+1;
            int n = b.Length+1;

            double [,] D = new double[m, n];

            for (int i = 0; i < m; i++)
                D[i, 0] = i;
            for (int j = 0; j < n; j++)
                D[0, j] = j;

            for (int i = 1; i < m; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    double cost = a[i-1] == b[j-1] ? 0 : replacement_cost;
                    D[i, j] = Math.Min
                    (
                        D[i - 1, j] + delete_cost,

                        Math.Min
                        (
                            D[i, j - 1] + insertion_cost,
                            D[i - 1, j - 1] + cost
                        )
                    );
                }
            }

            return 1 - (D[m-1, n-1] / (m + n));
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
