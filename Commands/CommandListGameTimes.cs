using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Fun;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandListGameTimes : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = (FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame");
            if (rg._playTimes.Count() == 0)
            {
                e.Channel.SendMessage("No gameplay time data!");
            }
            else
            {
                StringBuilder b = new StringBuilder("```");
                foreach (string k in rg._playTimes.Keys)
                {
                    b.AppendFormat("{0}: {1}\n", k, Utils.FormatUpdate(rg._playTimes[k]));
                }
                b.Append("```");
                e.Channel.SendMessage(b.ToString());
            }
        }
    }
}
