using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandLastLaunch : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase) // This is so hacky! I love it!
        {
            CommandManager cm = CommandManager.Instance;
            CommandNextLaunch cnl = (CommandNextLaunch)cm._commands.First(c => c.ToString().EndsWith("CommandNextLaunch"));
            cnl.customInvoke(e, pub, fromPhrase);
        }

        public override string triggerPattern()
        {
            return @"%me%,? what (is|was) the last (rocket )?launch\??";
        }

    }
}
