using DiscordBot.Extensions;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Logging
{
    public enum LogLevel
    {
        INFO,
        INF,
        WARNING,
        WRN,
        ERROR,
        ERR,
        DEBUG,
        DBG
    }

    public class Logger
    {

        public string Name { get; private set; }
        public string LogPath { get; private set; }
        private bool alwaysPrefix = false;

        public Logger(String name) : this(name, Path.Combine(Utils.getApplicationEXEFolderPath(), "logs", String.Format("{0}.log", name))) { }

        public Logger(String name, String path)
        {

            this.Name = name;
            this.LogPath = path;

            // Check if the logs directy exists, if not, make it so.
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            // At this point, everything is save to use, so we can start writing stuff to the log
            this.LogToEngineAndModule("Logger created with name '{0}' -- File path: {1}", name, path);

        }

        public Logger createSubLogger(string name)
        {
            Logger l = new Logger(name, this.LogPath);
            l.alwaysPrefix = true;
            return l;
        }

        public static void ArchiveAndRemoveOldLogs()
        {
            string archivePath = Path.Combine(Utils.getApplicationEXEFolderPath(), "log_archives");
            Directory.CreateDirectory(archivePath);
            string archiveName = String.Format("LOGS-{0}.zip", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            string fullArchivePath = Path.Combine(archivePath, archiveName);

            ZipFile.CreateFromDirectory(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs"), fullArchivePath, CompressionLevel.Fastest, false, Encoding.UTF8);

            string[] files = Directory.GetFiles(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs"));
            foreach (string f in files)
                File.Delete(f);
            foreach (string d in Directory.GetDirectories(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs")))
                Directory.Delete(d, true);

        }

        public void LogToEngineAndModule(string message, params object[] fillers)
        {
            LogToEngineAndModule(message, LogLevel.INFO, fillers);
        }

        public void LogToEngineAndModule(string message, LogLevel level, params object[] fillers)
        {
            Log(message, level, fillers);
            if (this.Name.EqualsIgnoreCase("program")) { return; }
            string _message = string.Format("[{0}] {1}", this.Name.ToUpper(), message);
            Program.Instance.Logger.Log(_message, level, fillers);
        }

        public void Log(string message, params object[] fillers)
        {
            Log(message, LogLevel.INFO, fillers);
        }

        public void Log(string message, LogLevel level, params object[] fillers)
        {

            // Before we do anything, lock to prevent conflicts

            lock (this)
            {

                string _msg = String.Format(message, fillers);
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

                string @string = string.Empty;
                if (this.alwaysPrefix)
                    @string = String.Format("[{0} / {1}] {2} [{4}] {3}", time, uptime_time, type, _msg, this.Name.ToUpper());
                else 
                    @string = String.Format("[{0} / {1}] {2} {3}", time, uptime_time, type, _msg);

#if DEBUG
                Console.WriteLine(@string);
#endif

                // Write the juicy goodness to file
                using (StreamWriter w = new StreamWriter(this.LogPath, true, Encoding.UTF8))
                {
                    w.WriteLine(@string);
                }


            }
        }

    }
}
