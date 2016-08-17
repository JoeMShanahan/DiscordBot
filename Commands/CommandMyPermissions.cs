using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandMyPermissions : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            CommandPermissionLevel[] perms = Utils.getPermissionsListForUser(e.User, e.Server);
            StringBuilder sb = new StringBuilder();
            sb.Append(e.User.Mention + ", you have the following bot permissions:");
            sb.AppendLine("```");
            foreach (CommandPermissionLevel l in perms)
                sb.AppendLine(l.ToString().Substring(l.ToString().LastIndexOf(".") + 1));
            sb.AppendLine("```");
            if (pub) e.Channel.SendMessage(sb.ToString());
            else e.User.SendMessage(sb.ToString());
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "myperms", "perms", "permslist" };
        }
    }
}
