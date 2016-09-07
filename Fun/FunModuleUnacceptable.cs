using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Fun
{
    public class FunModuleUnacceptable : FunModuleBase, IFunModule
    {

        public override void onMessageReceived(MessageEventArgs e)
        {

            if (e.Message.Text.Contains("unacceptable") && (e.Server.Id == 213292578093793282 || e.Server.Id == 212605353827762177))
            {
                e.Channel.SendMessage("https://www.youtube.com/watch?v=aaSRYecKaqc");
            }

        }

    }
}
