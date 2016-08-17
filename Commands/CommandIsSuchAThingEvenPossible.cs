using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandIsSuchAThingEvenPossible : CommandSimpleReplyBase, ICommand
    {

        public CommandIsSuchAThingEvenPossible() : base("https://i.imgur.com/iXtChhJ.gifv", true) { }

    }

}
