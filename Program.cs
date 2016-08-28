using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using System.IO;
using DiscordBot.Config;
using DiscordBot.Commands;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using DiscordBot.Fun;

namespace DiscordBot
{
    class Program
    {

        public bool IS_RUNNING = false;
        public DateTime STARTUP_TIME;
        public DiscordClient client;
        public BotConfig _config;
        public static Program Instance { get; private set; }
        public Logger Logger { get; private set; }
        public CommandManager commandManager { get; private set; }
        public ServerLogManager serverLogManager { get; private set; }
        public MessageLogger messageLogger { get; private set; }
        public FunManager funManager { get; private set; }

        static void Main(string[] args)
        {

            Program p = new Program();

        }

        public TimeSpan getUptime()
        {
            return (TimeSpan)(DateTime.Now - this.STARTUP_TIME);
        }

        public Program()
        {

            Instance = this;

            if (Directory.GetFiles(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs")).Length > 0)
                Logger.ArchiveAndRemoveOldLogs();

            this.STARTUP_TIME = DateTime.Now;
            this.Logger = new Logger("Program");
            this.Logger.Log("Checking for config directories and creating them if they don't exist.");

            // Set up config
            Directory.CreateDirectory("config/");
            if (!File.Exists("config/config.cfg"))
                File.WriteAllText("config/config.cfg", JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented));
            _config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config/config.cfg"));

            if (_config.botAPIToken.Equals("YOUR.TOKEN.HERE"))
            {
                Console.WriteLine("NO BOT TOKEN IS SET IN THE CONFIG, CANNOT RUN.");
                Console.WriteLine("PLEASE SET UP YOUR config/config.cfg FILE BEFROE RUNNING THE BOT AGAIN.");
            }
            else
            {
                this.Logger.Log("Initialising ServerLogManager");
                this.serverLogManager = new ServerLogManager();

                this.Logger.Log("Setting up DM logger");
                this.messageLogger = new MessageLogger(Path.Combine(Utils.getApplicationEXEFolderPath(), "logs", "DMs"));

                this.Logger.Log("Initialising CommandManager");
                // Set up command manager
                this.commandManager = new CommandManager();

                this.Logger.Log("Initialising FunManager");
                this.funManager = new FunManager();

                this.Logger.Log("Starting DiscordClient");
                this.client = new DiscordClient(new DiscordConfigBuilder()
                {
                    MessageCacheSize = 10,
                    ConnectionTimeout = 60000,
                    LogLevel = LogSeverity.Warning,
                    LogHandler = LogDebugMessage
                });

                this.IS_RUNNING = true;
                this.client.MessageReceived += MessageReceived;
                this.client.JoinedServer += JoinedServer;
                this.client.ServerAvailable += ServerAvailable;
                this.client.ServerUnavailable += ServerUnavailable;
                this.client.LeftServer += ServerUnavailable;
                this.client.UserJoined += (s, e) => UserJoinedOrLeft(s, e, true);
                this.client.UserLeft += (s, e) => UserJoinedOrLeft(s, e, false);
                this.client.UserUpdated += UserUpdated;
                /*this.client.MessageSent += MessageSent;
                this.client.MessageAcknowledged += MessageSent;*/ // <-- This and the one above it do NOTHING. They're not even implemented in the library
                this.client.MessageUpdated += MessageUpdated;

                this.client.ExecuteAndWait(async () =>
                {
                    await this.client.Connect(_config.botAPIToken); // TODO: Move to config

#if RELEASE
                await Task.Delay(150000).ConfigureAwait(false);
#else
                    await Task.Delay(1000).ConfigureAwait(false);
#endif



                });
                this.Logger.Log("No longer connected, terminating process.", Logging.LogLevel.WARNING);
            }
        }

        private void MessageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            bool isDM = e.Server == null;

            if (isDM)
            { // Private messages

                this.messageLogger.Log(e.Channel, "{0} [{1}] updated message '{2}' to '{3}'", e.User.Name, e.User.Id, e.Before.Text, e.After.Text);
            }
            else
            {
                this.serverLogManager.getLoggerForServerID(e.Server.Id).Log(e.Channel, "{0} [{1}] updated message '{2}' to '{3}' in channel '{4}' [{5}] on server '{6}' [{7}]", e.User.Name, e.User.Id, e.Before.Text, e.After.Text, e.Channel.Name, e.Channel.Id, e.Server.Name, e.Server.Id);
            }
        }

        private void MessageSent(object sender, MessageEventArgs e)
        {
            // This is only used to log replies in private messages to the correct place
            Console.WriteLine("Message sent ping");
            Utils.sendToDebugChannel("{0} // {1}: {2}", e.User.Name, e.User.Id, e.Message.Text);
        }

        private void UserUpdated(object sender, UserUpdatedEventArgs e)
        {
            if (!this._config.logUserUpdates) { return; }
            MessageLogger sl = this.serverLogManager.getLoggerForServer(e.Server);
            string logMsg = String.Format("Update on user '{0}' [{1}] from server '{3}' [{4}]: {2}", e.Before.Name, e.Before.Id, String.Format("{0} -> {1}, {2} -> {3}, {4} -> {5}, {6} -> {7}", e.Before.Name, e.After.Name, e.Before.Nickname, e.After.Nickname, e.Before.CurrentGame.GetValueOrDefault().Name, e.After.CurrentGame.GetValueOrDefault().Name, e.Before.Status.ToString(), e.After.Status.ToString()), e.Server.Name, e.Server.Id);
            sl.Log(logMsg);
        }

        private void UserJoinedOrLeft(object sender, UserEventArgs e, bool isJoin)
        {
            MessageLogger sl = this.serverLogManager.getLoggerForServer(e.Server);
            string logMsg = String.Format("{2} -> '{0}' [{1}]", e.User.Name, e.User.Id, isJoin?"JOIN":"LEAVE");
            sl.Log(logMsg);
        }

        private void ServerUnavailable(object sender, ServerEventArgs e) // Fires if the bot is no longer authorised to be on the server (I think) - We also use it to handle when the bot leaves a server
        {
            MessageLogger sl = this.serverLogManager.getLoggerForServer(e.Server);
            string logMsg = String.Format("Server unavailable: '{0}' [{1}]", e.Server.Name, e.Server.Id);
            sl.Log(logMsg);
            this.Logger.Log(logMsg);
            this.serverLogManager.removeLoggerForServer(e.Server.Id);
        }

        private void ServerAvailable(object sender, ServerEventArgs e)
        {
            MessageLogger sl = this.serverLogManager.createLoggerForServer(e.Server);
            string logMsg = String.Format("Server available: '{0}' [{1}]", e.Server.Name, e.Server.Id);
            sl.Log(logMsg);
            this.Logger.Log(logMsg);
        }

        private void JoinedServer(object sender, ServerEventArgs e)
        {
            MessageLogger sl = this.serverLogManager.createLoggerForServer(e.Server);
            string logMsg = String.Format("Joined server '{0}' [{1}]", e.Server.Name, e.Server.Id);
            sl.Log(logMsg);
            this.Logger.Log(logMsg);
        }

        private void LogDebugMessage(object sender, LogMessageEventArgs e)
        {
            /*if (this._config.USE_DEBUG_CHANNEL && this.client.Servers.Any())
            {

                if (!e.Message.Contains(this._config.DEBUG_CHANNEL_NAME))
                {
                    try
                    {
                        Server debug_server = this.client.Servers.First(s => s.Name.Equals(this._config.DEBUG_CHANNEL_SERVER_NAME));
                        Channel debug_channel = debug_server.TextChannels.First(c => c.Name.Equals(this._config.DEBUG_CHANNEL_NAME));
                        debug_channel.SendMessage(String.Format("[{0}] {1}", e.Severity, e.Message));
                    }
                    catch { Console.WriteLine("Cannot send messages to debug channel because it wasn't found."); }
                }
            }*/
            this.Logger.Log("[{0}] {1} / {2}", e.Severity, "", e.Message);

        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            this.funManager.onMessageReceived(e);
            bool isDM = e.Server == null;

            if (isDM)
            { // Private messages

                this.messageLogger.Log(e.Channel, "{0} [{1}]: {2}", e.User.Name, e.User.Id, e.Message.Text);
            }
            else
            {
                this.serverLogManager.getLoggerForServerID(e.Server.Id).Log(e.Channel, "{0} [{1}]: {2}", e.User.Name, e.User.Id, e.Message.Text);
#if DEBUG
            this.Logger.Log("[{0}] {1} [{4}]@{3}: {2}", e.Server, e.User, e.Message.Text, e.Channel, e.User.Id);
#endif
            }

            this.commandManager.invokeMatchingPhraseCommands(e);
            if (this._config.commandTriggerCharacters.Contains(e.Message.Text.Substring(0, 1)))
            {
                string command = e.Message.Text.Substring(1).Split(' ')[0];
                this.commandManager.invokeCommandsFromName(command, e, isDM);
            }
            else if (!isDM && e.Message.Text.Contains("unacceptable") && e.Server.Id == 213292578093793282)
            {
                e.Channel.SendMessage("https://www.youtube.com/watch?v=aaSRYecKaqc");
            }

        }
    }
}
