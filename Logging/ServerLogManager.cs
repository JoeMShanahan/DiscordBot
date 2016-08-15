using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Logging
{

    public class NoSuchServerException : Exception
    {
        public NoSuchServerException(string message) : base(message)
        {
        }
    }

    public class ServerLogManager
    {

        public Dictionary<ulong, ServerLogger> _loggers = new Dictionary<ulong, ServerLogger>();
        public static ServerLogManager Instance { get; private set; }
        public Logger Logger { get; private set; }

        public ServerLogManager()
        {
            Instance = this;
            this.Logger = new Logger("ServerLogManager");
        }

        public ServerLogger createLoggerForServer(Server s)
        {
            if (this._loggers.ContainsKey(s.Id))
            {
                this.Logger.Log("Attempted to create a logger for server '{0}' [{1}], but it already exists! This normally happens if we're invited to a new, or told to leave an existing channel", LogLevel.WARNING, s.Name, s.Id);
                return this._loggers[s.Id];
            }
            this.Logger.Log("Creating server logger for server '{0}' [{1}]", s.Name, s.Id);
            ServerLogger sl = new ServerLogger(s);
            this._loggers.Add(s.Id, sl);
            return sl;
        }

        public void removeLoggerForServer(ulong id)
        {
            this.Logger.Log("Removing server logger for server ID {0} if it exists", id);
            this._loggers.Remove(id);
        }

        public ServerLogger getLoggerForServerID(ulong id)
        {
            if (this._loggers.ContainsKey(id))
            {
                return this._loggers[id];
            }
            else
            {
                throw new NoSuchServerException(String.Format("The server ID '{0}' does not exist.", id));
            }
        }

        public ServerLogger getLoggerForServer(Server server)
        {
            return getLoggerForServerID(server.Id);
        }
    }
}
