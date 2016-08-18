﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class PhraseCommandSimpleReplyBase : ICommand
    {

        string reply = string.Empty;

        public PhraseCommandSimpleReplyBase(string reply)
        {
            this.reply = reply;
        }

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

        public virtual void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            e.Channel.SendMessage(this.reply);
        }

        public virtual string triggerPattern()
        {
            return string.Empty;
        }
    }
}
