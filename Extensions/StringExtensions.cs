using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Extensions
{
    public static class StringExtensions
    {

        public static bool EqualsIgnoreCase(this string s, string compare)
        {
            return s.ToLower().Equals(compare.ToLower());
        }

        public static bool EndsWithIgnoreCase(this string s, string compare)
        {
            return s.ToLower().EndsWith(compare.ToLower());
        }

        public static bool ContainsIgnoreCase(this string s, string compare)
        {
            return s.ToLower().Contains(compare.ToLower());
        }

        public static string FromNthDeliminator(this string orig, int n, char delim)
        {
            string[] data = orig.Split(delim);
            if (Math.Abs(n) > data.Length)
                throw new IndexOutOfRangeException();
            string @out = string.Empty;
            if (n > 0)
            {
                for (int x = n; x < data.Length; x++)
                {
                    @out += (@out.Length > 0 ? delim.ToString() : "") + data[x];
                }
            }
            else
            {
                for (int x = 0; x < data.Length - Math.Abs(n); x++)
                {
                    @out += (@out.Length > 0 ? delim.ToString() : "") + data[x];
                }
            }
            return @out;
        }

        public static string[] SplitWithEscapes(this string str, char split)
        {
            var regex = new Regex(String.Format(@"(?<!\\){0}", split.ToString()));
            return regex.Split(str);
        }

    }
}
