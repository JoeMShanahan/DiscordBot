using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandLeaveServer : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            try
            {
                ulong serverID = Convert.ToUInt64(e.Message.Text.Split(' ')[1]);
                Server s = Program.Instance.client.Servers.First(a => a.Id == serverID);
                s.Leave();
                Utils.sendToDebugChannel("Left server '{0}' [{1}] at request of owner '{2}' [{3}]", s.Name, s.Id, e.User.Name, e.User.Id);
            }
            catch (InvalidCastException)
            {
                e.Channel.SendMessage("The specified channel ID is invalid");
            }
            catch (IndexOutOfRangeException)
            {
                e.Channel.SendMessage("No server ID was specified.");
            }
            catch
            {
                e.Channel.SendMessage("An unknown error occured while trying to leave server");
            }
        }
    }
}
