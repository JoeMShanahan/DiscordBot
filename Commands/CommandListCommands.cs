using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Reflection;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class CommandListCommands : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            StringBuilder b = new StringBuilder();
            b.Append("Commands: ```");
            foreach (ICommand c in CommandManager.Instance._commands)
                b.AppendFormattedLine("\t{0}", c.ToString());
            e.Channel.SendMessage(String.Format("{0}```", b.ToString()));

        }
    }
}
