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
    public class CommandDebugTimePlayed : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "dtp" };
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = (FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame");
            if (rg.isPlayingGame)
            {
                Int64 timePlayed = (Int64)(DateTime.Now - rg.startedPlaying).TotalSeconds;
                e.Channel.SendMessageFormatted("**{1}**: {0}\n{2} vs {3}", timePlayed, rg.currentGame, rg.currentGame, Program.Instance.client.CurrentGame.Name);
            }
            else
                e.Channel.SendMessage("Not playing a game.");
        }
    }
}
