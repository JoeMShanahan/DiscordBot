using System;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandAmIOwner : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase)
        {
            string reply = String.Format("{0}, you are {1}owner.", e.User.Mention, Utils.userIsOwner(e.User) ? "" : "NOT ");
            if (pub) e.Channel.SendMessage(reply);
            else e.User.SendMessage(reply);
        }
    }
}
