using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandQuit : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            //Utils.sendToDebugChannel("User '{0}' [{1}] has requested that I disconnect from discord.", e.User, e.User.Id);
            Program.Instance._config.saveConfig();
            Program.Instance.client.Disconnect();
        }
    }
}
