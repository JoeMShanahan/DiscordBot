using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Extensions
{
    public static class StringBuilderExtensions
    {


        public static StringBuilder AppendFormattedLine(this StringBuilder sb, string format, params object[] replacers)
        {
            string @string = String.Format(format, replacers);
            sb.AppendLine(@string);

            return sb;
        }

    }
}
