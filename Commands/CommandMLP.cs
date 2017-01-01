using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class CommandMLP : CommandSimpleReplyBase, ICommand
    {

        public CommandMLP() : base("Neigh. :horse:", true) { }

        public override string helpText()
        {
            return "Neigh.";
        }
    }
}
