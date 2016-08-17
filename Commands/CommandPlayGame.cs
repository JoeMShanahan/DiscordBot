using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandPlayGame : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            string game = Program.Instance.funManager.onMessageReceived(e, true);
            Utils.sendToDebugChannel("New Game: {0}", game);
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "playagame", "newgame", "switchgame" };
        }
    }
}
