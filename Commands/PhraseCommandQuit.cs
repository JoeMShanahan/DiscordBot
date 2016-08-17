using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class PhraseCommandQuit : PhraseCommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            Program.Instance.commandManager.invokeCommandsFromName("quit", e);
        }

        public override string triggerPattern()
        {
            return @"%me%,? (quit|qqq|disconnect|terminate|die|exit)";
        }
    }
}
