using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using DiscordBot.Extensions;
using DiscordBot.Fun;

namespace DiscordBot.Commands
{
    public class CommandBlacklistGame : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            string gameList = e.Message.Text.FromNthDeliminator(1, ' ');
            string[] games = gameList.SplitWithEscapes(',');

            FunModuleRandomGame rg = (FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame");

            foreach (string s in games)
            {
                if (rg._gameNames.Contains(s))
                    rg._gameNames.Remove(s);
                if (!rg._blacklistedGames.Contains(s.Replace("\\,", ",")))
                    rg._blacklistedGames.Add(s.Replace("\\,", ","));
                rg.saveGameBlacklist();
                rg.saveGameList();

            }
        }
    }
}
