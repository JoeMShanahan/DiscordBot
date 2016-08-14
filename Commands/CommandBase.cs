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

        public virtual bool goesOnCooldown()
        {
            return true;
        }

        public virtual int cooldownLength()
        {
            return Program.Instance._config.commandCooldownSecs;
        }

        public virtual string[] getCommandAliases()
        {
            return new string[0];
        }

    }
}
