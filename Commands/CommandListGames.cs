using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Fun;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class CommandListGames : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = (FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame");
            StringBuilder sb = new StringBuilder("```");
            foreach (string s in rg._gameNames)
            {
                if (sb.ToString().Length + (s.Length + 3) > 2000) // Fix for games list exceeding 2,000 characters
                {
                    sb.Append("```");
                    e.Channel.SendMessageFormatted("{0}", sb.ToString());
                    sb.Clear();
                    sb.Append("```");
                }
                sb.AppendLine($"{s}");
            }
            e.Channel.SendMessageFormatted("{0}```", sb.ToString());
        }
    }
}
