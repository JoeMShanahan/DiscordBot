using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class PhraseCommandThanks : PhraseCommandBase, ICommand
    {
        public override string helpText()
        {
            return "Thanks!";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = true)
        {
            e.Channel.SendMessageFormatted("No problem, {0} {1}", e.User.Mention, Utils.emojiWithRandomSkintone("thumbsup"));
        }

        public override string triggerPattern()
        {
            return @"%me%,? thanks|thanks,? %me%";
        }
    }
}
