using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandLaunchSchedule : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            e.Channel.SendMessage("https://ipeer.auron.co.uk/launchschedule/");
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "schedule" };
        }
    }
}
