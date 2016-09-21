using Discord;
using DiscordBot.Extensions;
using DiscordBot.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Utilities
{

    public class DebugServerNotFoundException : Exception { }
    public class DebugChannelNotFoundException : Exception { }
    public class DebugChannelNotEnabledException : Exception { }

    public static class Utils
    {

        private static DateTime _userCountCacheTime = new DateTime(1970, 1, 1);
        private static int _lastCachedUserCount = 0;

        public static bool userIsOwner(User u)
        {
            return Program.Instance._config.botOwnerAccountIDs.Contains(u.Id);
        }

        public static bool userIsServerAdmin(User user)
        {
            try
            {
                return user.ServerPermissions.Administrator;
            }
            catch { return false; }
        }

        public static bool userIsServerOwner(User user, Server s)
        {
            if (s == null) return false;
            return s.Owner == user;
        }

        public static Commands.CommandPermissionLevel[] getPermissionsListForUser(User u, Server s)
        {

            List<Commands.CommandPermissionLevel> _perms = new List<Commands.CommandPermissionLevel>();
            if (userIsOwner(u)) { _perms.Add(Commands.CommandPermissionLevel.BOT_OWNER); }
            if (getDebugServer().Users.Contains(u) && getDebugServer().Users.First(a => a.Id == u.Id).Roles.Contains(getDebugServer().Roles.First(r => r.Name.Equals("Bot Admin")))) { _perms.Add(Commands.CommandPermissionLevel.BOT_ADMIN); } // Crikey
            if (userIsServerOwner(u, s)) { _perms.Add(Commands.CommandPermissionLevel.SERVER_OWNER); }
            if (userIsServerAdmin(u)) { _perms.Add(Commands.CommandPermissionLevel.SERVER_ADMIN); }
            _perms.Add(Commands.CommandPermissionLevel.NORMAL_USER);

            return _perms.ToArray();

        }

        public static string getAssemblyBuildTime()
        {
            AssemblyBuildTime att = (AssemblyBuildTime)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyBuildTime));
            return att.BuildTime;
        }

        public static Commands.CommandPermissionLevel getCommandPermissionlevelForUser(User u, Server s)
        {
            if (userIsOwner(u)) { return Commands.CommandPermissionLevel.BOT_OWNER; }
            if (getDebugServer().Users.Contains(u) && getDebugServer().Users.First(a => a.Id == u.Id).Roles.Contains(getDebugServer().Roles.First(r => r.Name.Equals("Bot Admin")))) { return Commands.CommandPermissionLevel.BOT_ADMIN; }
            else if (s == null) { return Commands.CommandPermissionLevel.NORMAL_USER; }
            else if (userIsServerOwner(u, s)) { return Commands.CommandPermissionLevel.SERVER_OWNER; }
            else if (userIsServerAdmin(u)) { return Commands.CommandPermissionLevel.SERVER_ADMIN; }
            else return Commands.CommandPermissionLevel.NORMAL_USER;
        }

        public static Server getDebugServer()
        {
            try
            {
                return Program.Instance.client.Servers.First(s => s.Name.Equals(Program.Instance._config.debugChannelServerName));
            }
            catch
            {
                throw new DebugServerNotFoundException();
            }
        }

        public static Channel getDebugChannel()
        {
            try
            {
                return getDebugServer().TextChannels.First(c => c.Name.Equals(Program.Instance._config.debugChannelName));
            }
            catch (DebugServerNotFoundException e) { throw e; }
            catch
            {
                throw new DebugChannelNotFoundException();
            }
        }

        public static void sendToDebugChannel(string format, params object[] replacers)
        {

            string message = String.Format(format, replacers);
            if (!Program.Instance._config.useDebugChannel) { throw new DebugChannelNotEnabledException(); }
            try
            {
                Server debug_server = getDebugServer();
                Channel debug_channel = getDebugChannel();
                //Console.WriteLine("{0} {1}", debug_server, debug_channel);
                debug_channel.SendMessage(message.ToString());
            }
            catch (DebugServerNotFoundException) { Console.WriteLine("Cannot send message to debug channel because the debug server was not found"); }
            catch (DebugChannelNotFoundException) { Console.WriteLine("Cannot send message to debug channel because the specified channel was not found"); }
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

        public static string FormatUpdate(long l)
        {
            return FormatUptime(TimeSpan.FromSeconds(l));
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

        internal static void LogException(string clazz, Exception exception)
        {
            string exceptionText = String.Format("[**EXCEPTION**] Exception occured in {1}:\n```{0}```", exception.ToString(), clazz);
            try
            {
                Utils.sendToDebugChannel(exceptionText);
            }
            catch { }
            Program.Instance.Logger.Log("{0}", exceptionText.Replace(new string[] { "[**EXCEPTION**]", "```" }, new string[] { "[EXCEPTION]", "" }), LogLevel.ERROR);

        }

        public static bool isUserIgnored(UInt64 id)
        {
            return Program.Instance._config.ignoredUsers.Contains(id);
        }

        public static bool isUserIgnored(string command, MessageEventArgs e)
        {
            if (isUserIgnored(e.User.Id))
            {
                string logMsg = String.Format("Ignoring command '{0}' from user '{1}' [{2}] as they are on the ignore list", command, e.User.Name, e.User.Id);

                if (/*e.Server == null*/e.Channel.IsPrivate)
                    Program.Instance.messageLogger.Log(e.Channel, logMsg, LogLevel.WARNING);
                else
                    Program.Instance.serverLogManager.getLoggerForServer(e.Server).Log(e.Channel, logMsg, LogLevel.WARNING);

                Program.Instance.Logger.Log(logMsg, LogLevel.WARNING);
                return true;
            }
            return false;
        }

        public static string getBotVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }

        // Not my code!
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string FormatBytes(Int64 value)
        {
            if (value < 0) { return "-" + FormatBytes(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        public static string emojiWithRandomSkintone(string v)
        {
            Random r = new Random(getEpochTime());
            int tone = r.RealNext(1, 5);
            return string.Format(":{0}::skin-tone-{1}:", v, tone);
        }

        public static int getUserCount()
        {
            int users = 0;
            foreach (Server s in Program.Instance.client.Servers)
                users += s.UserCount;
            return users;
        }

        public static int getUserCountCached()
        {
            if (DateTime.Compare(_userCountCacheTime.AddHours(1), DateTime.Now) <= 0)
            {
                _userCountCacheTime = DateTime.Now;
                _lastCachedUserCount = getUserCount();
            }
            return _lastCachedUserCount;
        }
    }
}
