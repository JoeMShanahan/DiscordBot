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
    public class PhraseCommandHello : PhraseCommandBase, ICommand
    {
        public override string helpText()
        {
            return "Hello :)";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = true)
        {
            e.Channel.SendMessageFormatted("Hello, {0} {1}", e.User.Mention, Utils.emojiWithRandomSkintone("wave"));
        }

        public override string triggerPattern()
        {
            return @"%me%,? (hello|hi|o\/|hola|sup)|(hello|hi|o\/|hola|sup),? %me%";
        }
    }
}
