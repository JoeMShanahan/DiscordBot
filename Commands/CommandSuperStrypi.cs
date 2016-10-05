using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandSuperStrypi : CommandSimpleReplyBase, ICommand
    {

        public CommandSuperStrypi() : base("https://i.imgur.com/7xyikQa.gif", true) { }

        public override string[] getCommandAliases()
        {
            return new string[] { "strypi" };
        }

        public override string helpText()
        {
            return "You spin me right round baby, right round";
        }
    }
}
