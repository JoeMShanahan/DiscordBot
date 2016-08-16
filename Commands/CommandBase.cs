using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public abstract class CommandBase : ICommand // I know we don't need to implement the interface here, I do it just to make sure I have all the methods in the base class.
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
