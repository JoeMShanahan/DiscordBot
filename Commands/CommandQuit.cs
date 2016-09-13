using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using DiscordBot.Fun;

namespace DiscordBot.Commands
{
    public class CommandQuit : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            //Utils.sendToDebugChannel("User '{0}' [{1}] has requested that I disconnect from discord.", e.User, e.User.Id);
            Program.Instance._config.saveConfig();
            ((FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame")).saveCurrentGameTime();
            Program.Instance.client.Disconnect();
        }

        public override bool goesOnCooldown()
        {
            return false;
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "qqq", "die", "terminate", "exit", "disconnect" };
        }
    }
}
