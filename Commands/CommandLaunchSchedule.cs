using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandLaunchSchedule : CommandSimpleReplyBase, ICommand
    {

        public CommandLaunchSchedule() : base("https://ipeer.auron.co.uk/launchschedule/", true) { }

        public override string[] getCommandAliases()
        {
            return new string[] { "schedule" };
        }

        public override string helpText()
        {
            return "Returns a link to the launch schedule page";
        }
    }
}
