using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class PhraseCommandTest : PhraseCommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            e.Channel.SendMessage(String.Format("Test received, {0}", e.User.Mention));
        }

        public override string triggerPattern()
        {
            return @"%me%,? t[ea]st";
        }
    }
}
