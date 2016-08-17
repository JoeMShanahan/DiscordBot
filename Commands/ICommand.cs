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

        void invoke(MessageEventArgs e, bool pub);
        CommandPermissionLevel getRequiredPermissionLevel();
        int cooldownLength();
        bool goesOnCooldown();
        string[] getCommandAliases();
        string triggerPattern();

    }
}
