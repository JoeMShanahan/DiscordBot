using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandTest : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            e.Channel.SendMessage("Received.");
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "t", "tt", "test2" };
        }
    }
}
