using Discord;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Logging
{

    // This class allows logging of server related behaviour, including channel creation/deletion, user message and user joins/leaves for anti-abuse and debugging purposes.
    // This class is not created to enable the bot owners to "spy" on users of the bot and is purely in place to allow owners to track down and take action against bot abuse or address issues with bot behaviour caused by bugs or glitches.
    // For more information, please see https://github.com/iPeer/DiscordBot/blob/master/Privacy.md

    public class MessageLogger
    {

        public string logDir = string.Empty;

        public MessageLogger(Server s) : this(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs", "servers", String.Format("{0} [{1}]", s.Name, s.Id))) { }

        public MessageLogger(string directoryPath)
        {
            // Create the folders for this server

            // main server folder

            // Note that we put the ID of the server in the folder name. This is to help find servers that change their names.
            // We could just use the server ID for foldername, but that makes finding a specific server difficult. The same formatting is used for channel log files.
            this.logDir = directoryPath;
            Directory.CreateDirectory(this.logDir);
        }

        public void Log(Channel c, string message, LogLevel level, params object[] fillers) { Log(String.Format("{0} [{1}]", c.Name, c.Id), message, level, fillers); }
        public void Log(Channel c, string message, params object[] fillers) { Log(String.Format("{0} [{1}]", c.Name, c.Id), message, LogLevel.INFO, fillers); }
        //public void Log(User u, string message, LogLevel level, params object[] fillers) { if (u.Id == Program.Instance.client.CurrentUser.Id) { return; }; Log(String.Format("{0} [{1}]", u.Name, u.Id), message, level, fillers); }
        //public void Log(User u, string message, params object[] fillers) { if (u.Id == Program.Instance.client.CurrentUser.Id) { return; }; Log(String.Format("{0} [{1}]", u.Name, u.Id), message, LogLevel.INFO, fillers); }
        public void Log(string message, params object[] fillers) { Log("server", message, LogLevel.INFO, fillers); }
        public void Log(string channel, string message, params object[] fillers) { Log(channel, message, LogLevel.INFO, fillers); }
        public void Log(string fileName, string message, LogLevel level, params object[] fillers)
        {

            string logPath = Path.Combine(this.logDir, string.Format("{0}.log", fileName));

            string msg = String.Format(message, fillers);
            string type = "INF";
            switch (level)
            {
                case LogLevel.ERR:
                case LogLevel.ERROR:
                    type = "ERR";
                    break;
                case LogLevel.DBG:
                case LogLevel.DEBUG:
                    type = "DBG";
                    break;
                case LogLevel.WRN:
                case LogLevel.WARNING:
                    type = "WRN";
                    break;
                default:
                    type = "INF";
                    break;
            }

            string time = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
            string uptime_time = Utils.FormatUptime(Program.Instance.getUptime());

            string @string = String.Format("[{0} / {1}] {2} {3}", time, uptime_time, type, msg);

#if DEBUG
            string dbg_string = String.Format("[{0} / {1}] {2} {3}", time, uptime_time, type, msg);
            Console.WriteLine(dbg_string);
#endif

            // Write the juicy goodness to file

            using (StreamWriter w = File.AppendText(logPath))
            {
                w.WriteLine(@string);
            }

        }

    }
}
