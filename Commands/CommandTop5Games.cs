using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Extensions;
using DiscordBot.Fun;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandTop5Games : CommandBase, ICommand
    {
        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            bool top5 = e.Message.Text.Split(' ')[0].Substring(1).StartsWithIgnoreCase("top5");
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            Dictionary<string, long> games = new Dictionary<string, long>(rg._playTimes);
            IOrderedEnumerable<KeyValuePair<string, long>> sorted = games.OrderByDescending(k => k.Value);
            int x = 0;
            StringBuilder sb = new StringBuilder(String.Format("Top {0} games, ordered by playtime:\n", top5 ? 5 : 10));
            foreach (KeyValuePair<string, long> kvp in sorted)
            {
                if (x++ >= (top5 ? 5 : 10)) { break; }
                long time = kvp.Value;
                if (rg.currentGame.EqualsIgnoreCase(kvp.Key))
                    time += (long)(DateTime.Now - rg.startedPlaying).TotalSeconds;
                sb.AppendFormattedLine("**{0}**. {1}: {2}", x, kvp.Key, Utils.FormatUptime(TimeSpan.FromSeconds(time)));
            }
            e.Channel.SendMessage(sb.ToString());
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "top5", "top10", "top10games" };
        }

    }
}
