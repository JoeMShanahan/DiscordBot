using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Extensions;
using System.Text.RegularExpressions;

namespace DiscordBot.Commands
{
    public class CommandRegexTest : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            string @params = e.Message.RawText.Substring(e.Message.RawText.Split(' ')[0].Length + 1);
            string regex = @params.Split('|')[0];
            string str = @params.Split('|')[1];


            StringBuilder b = new StringBuilder();
            b.AppendFormattedLine("Matching regex pattern **{0}** against string **{1}**", regex, str);
            MatchCollection matches = Regex.Matches(str, regex);
            int _matches = matches.Count;
            b.AppendFormattedLine("```Matches: {0}", _matches);
            foreach (Match m in matches)
                b.AppendFormattedLine("\t{0}", m.Value);
            b.AppendLine("```");

            e.Channel.SendMessage(b.ToString());

        }

        public override string[] getCommandAliases()
        {
            return new string[] { "regex" };
        }
    }
}
