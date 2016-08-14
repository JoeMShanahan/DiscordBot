using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Threading;

namespace DiscordBot.Commands
{
    public class CommandLeave : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.SERVER_ADMIN;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            Thread t = new Thread(new ThreadStart(() => runThread(e)));
            t.IsBackground = false;
            t.Start();
        }

        public void runThread(MessageEventArgs e)
        {
            e.User.SendMessage("Okay, I'll leave :(\nIf you change your mind, you can invite me back using the following URL: <https://discordapp.com/oauth2/authorize?client_id=213442800996319243&scope=bot>");
            Thread.Sleep(1000);
            e.Server.Leave();
        }
    }
}
