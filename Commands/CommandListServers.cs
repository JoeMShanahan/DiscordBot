using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandListServers : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override string helpText()
        {
            return "Lists all currently connected servers. If -c is given, also lists all channels on the servers";
        }

        public override string usageText()
        {
            return "%c% [-c]";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {

            bool chans = (e.Message.Text.Split(' ').Length > 1 && e.Message.Text.Split(' ')[1].Equals("-c"));

            StringBuilder sb = new StringBuilder();
            sb.Append("```");

            List<Server> _servers = new List<Server>(Program.Instance.client.Servers);

            foreach (Server s in _servers)
            {

                sb.AppendLine(String.Format("{0} [{1}]", s.Name, s.Id));
                if (chans)
                    foreach (Channel c in s.AllChannels)
                        sb.AppendLine(String.Format("\t({2}{3}) {0} [{1}]", c.Name, c.Id, c.Type, (c == s.DefaultChannel ? ", default" : "")));

            }


            sb.Append("```");

            if (pub)
                e.Channel.SendMessage(sb.ToString());
            else
                e.User.SendMessage(sb.ToString());
        }
    }
}
