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

namespace DiscordBot
{
    class Program
    {

        public bool IS_RUNNING = false;
        public DiscordClient client;
        public BotConfig _config;
        public static Program Instance { get; private set; }
        public CommandManager commandManager;

        static void Main(string[] args)
        {

            Program p = new Program();

        }

        public Program()
        {

            Instance = this;

            // Set up config
            Directory.CreateDirectory("config/");
            if (!File.Exists("config/config.cfg"))
                File.WriteAllText("config/config.cfg", JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented));
            _config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config/config.cfg"));

            if (_config.BOT_TOKEN.Equals("YOUR.TOKEN.HERE"))
            {
                Console.WriteLine("NO BOT TOKEN IS SET IN THE CONFIG, CANNOT RUN.");
                Console.WriteLine("PLEASE SET UP YOUR config/config.cfg FILE BEFROE RUNNING THE BOT AGAIN.");
            }
            else
            {
                // Set up command manager
                this.commandManager = new CommandManager();

                this.client = new DiscordClient(new DiscordConfigBuilder()
                {
                    MessageCacheSize = 10,
                    ConnectionTimeout = 60000,
                    LogLevel = LogSeverity.Verbose,
                    LogHandler = LogDebugMessage
                });

                this.IS_RUNNING = true;
                this.client.MessageReceived += MessageReceived;

                this.client.ExecuteAndWait(async () =>
                {
                    await this.client.Connect(_config.BOT_TOKEN); // TODO: Move to config

#if RELEASE
                await Task.Delay(150000).ConfigureAwait(false);
#else
                    await Task.Delay(1000).ConfigureAwait(false);
#endif



                });
            }
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
            Console.WriteLine("[{0}] {1} / {2}", e.Severity, "", e.Message);

        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine("[{0}] {1} [{4}]@{3}: {2}", e.Server, e.User, e.Message.Text, e.Channel, e.User.Id);
            if (this._config.COMMAND_CHARACTERS.Contains(e.Message.Text.Substring(0, 1)))
            {
                this.commandManager.invokeCommandsFromName(e.Message.Text.Substring(1).Split(' ')[0], e);
            }
        }
    }
}
