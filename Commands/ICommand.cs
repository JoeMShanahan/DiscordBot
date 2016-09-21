using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public interface ICommand
    {
        void initialise();
        void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false);
        CommandPermissionLevel getRequiredPermissionLevel();
        int cooldownLength();
        bool goesOnCooldown();
        string[] getCommandAliases();
        string triggerPattern();

    }
}
