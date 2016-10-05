using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public abstract class PhraseCommandBase : ICommand
    {

        public abstract void invoke(MessageEventArgs e, bool pub, bool fromPhrase = true);
        public abstract string triggerPattern();


        public virtual int cooldownLength()
        {
            return Program.Instance._config.commandCooldownSecs;
        }

        public virtual string[] getCommandAliases()
        {
            return new string[0];
        }

        public virtual CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.NORMAL_USER;
        }

        public virtual bool goesOnCooldown()
        {
            return true;
        }

        public virtual void initialise() { }

        public abstract string helpText();
        public virtual string usageText() { return "%c%"; }
        public virtual string CommandName
        {
            get
            {
                string[] c = this.ToString().Split('.');
                string name = c[c.Length - 1];
                return name.Substring(13);
            }
        }
    }
}
