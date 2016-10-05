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

        public override string[] getCommandAliases()
        {
            return new string[] { "ll" };
        }

        public override string helpText()
        {
            return "If _search_ is not provided this command will return information on the last rocket launch. If _search_ is provided, it will return information on the first previous launch (if any) that matches _search_";
        }

        public override string usageText()
        {
            return "%c% [search]";
        }

    }
}
