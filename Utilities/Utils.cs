using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Utilities
{
    public static class Utils
    {

        public static bool userIsOwner(User u)
        {
            return Program.Instance._config.BOT_OWNERS.Contains(u.Id);
        }

        public static bool userIsServerAdmin(User user)
        {
            return user.ServerPermissions.Administrator;
        }

        public static bool userIsServerOwner(User user, Server s)
        {
            return s.Owner == user;
        }

        public static Commands.CommandPermissionLevel[] getPermissionsListForUser(User u, Server s)
        {

            List<Commands.CommandPermissionLevel> _perms = new List<Commands.CommandPermissionLevel>();

            if (userIsOwner(u)) { _perms.Add(Commands.CommandPermissionLevel.BOT_OWNER); }
            if (userIsServerOwner(u, s)) { _perms.Add(Commands.CommandPermissionLevel.SERVER_OWNER); }
            if (userIsServerAdmin(u)) { _perms.Add(Commands.CommandPermissionLevel.SERVER_ADMIN); }
            _perms.Add(Commands.CommandPermissionLevel.NORMAL_USER);

            return _perms.ToArray();

        }

        public static Commands.CommandPermissionLevel getCommandPermissionlevelForUser(User u, Server s)
        {
            if (userIsOwner(u)) { return Commands.CommandPermissionLevel.BOT_OWNER; }
            else if (userIsServerOwner(u, s)) { return Commands.CommandPermissionLevel.SERVER_OWNER; }
            else if (userIsServerAdmin(u)) { return Commands.CommandPermissionLevel.SERVER_ADMIN; }
            else return Commands.CommandPermissionLevel.NORMAL_USER;
        }

        public static void sendToDebugChannel(string format, params object[] replacers)
        {

            string message = String.Format(format, replacers);

            try
            {
                Server debug_server = Program.Instance.client.Servers.First(s => s.Name.Equals(Program.Instance._config.DEBUG_CHANNEL_SERVER_NAME));
                Channel debug_channel = debug_server.TextChannels.First(c => c.Name.Equals(Program.Instance._config.DEBUG_CHANNEL_NAME));
                Console.WriteLine("{0} {1}", debug_server, debug_channel);
                debug_channel.SendMessage(message.ToString());
            }
            catch { Console.WriteLine("Cannot send messages to debug channel because it wasn't found."); }
        }

    }
}
