using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Utilities
{
    public static class Utils
    {

        public static bool userIsOwner(User u)
        {
            return Program.Instance._config.botOwnerAccountIDs.Contains(u.Id);
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
                Server debug_server = Program.Instance.client.Servers.First(s => s.Name.Equals(Program.Instance._config.debugChannelServerName));
                Channel debug_channel = debug_server.TextChannels.First(c => c.Name.Equals(Program.Instance._config.debugChannelName));
                Console.WriteLine("{0} {1}", debug_server, debug_channel);
                debug_channel.SendMessage(message.ToString());
            }
            catch (NullReferenceException) { Console.WriteLine("Cannot send messages to debug channel because it wasn't found."); }
            catch (Exception _e) { Console.WriteLine("A message couldn't be sent to the debug channel because an exception occurred: {0}", _e.Message); }
        }

        public static string getWebPage(string address, bool ignoreCerts = false, SecurityProtocolType prot = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3)
        {
            string data = "";
            using (WebClient c = new WebClient())
            {
                ServicePointManager.SecurityProtocol = prot;
                if (ignoreCerts)
                    ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
                data = c.DownloadString(address);
                if (ignoreCerts)
                    ServicePointManager.ServerCertificateValidationCallback = null; // People have had mixed results with "resetting" it like this, but it's the only option we really have
            }
            return data;
        }

        public static string getApplicationEXEFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string FormatUptime(TimeSpan t)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", t.Days, t.Hours, t.Minutes, t.Seconds);
        }

        public static int getEpochTime()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }

    }
}
