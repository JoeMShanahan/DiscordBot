using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public abstract class CommandBase : ICommand
    {
        public abstract CommandPermissionLevel getRequiredPermissionLevel();
        public abstract void invoke(MessageEventArgs e, bool pub);

        public Boolean isOnCooldown(MessageEventArgs e)
        {
            return CommandManager.Instance.IsCommandOnCooldown(this, e);
        }

    }
}
