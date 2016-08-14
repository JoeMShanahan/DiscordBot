using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Config
{
    public class InvalidUpdateMethodException : Exception
    {
        public InvalidUpdateMethodException(string message) : base(message) { }
    }

    public class BotConfig
    {
        public string BOT_TOKEN { get; set; } = "YOUR.TOKEN.HERE"; // Bot's token (think password)

        public ulong[] BOT_OWNERS { get; set; } = new ulong[3]{ 1, 2, 3 }; // List of bot owner account IDs

        public bool NO_ADMIN_SPAM { get; set; } = false; // Can admins bypass command cooldowns and other anti-spam measures?

        public int COMMAND_COOLDOWN_SECS { get; set; } = 30; // How long does a command go on cooldown for after being used? (per server)

        public bool USE_DEBUG_CHANNEL { get; set; } = true; // SAhould the bot make use of a debug channel for information or error logging?

        public string DEBUG_CHANNEL_SERVER_NAME { get; set; } = "SERVER.NAME.THE.DEBUG.CHANNEL.IS.ON"; // The name of the server that the debug channel is located on

        public string DEBUG_CHANNEL_NAME { get; set; } = "DEBUG.CHANNEL.NAME"; // The name of the debug channel

        public bool LOG_PERMISSION_ERRORS { get; set; } = true; // Should we log permission errors to the debug channel?

        public string COMMAND_CHARACTERS { get; set; } = "!~#.$"; // What character does a command have to start with to be treated as a command?

        public string PUBLIC_COMMAND_CHARACTERS { get; set; } = "~#$"; // Which of the above command characters are treated as "public" commands (replies sent to channel, not DM)? Note that characters listed here, but not in COMMAND_CHARACTERS will have no effect.

        public bool ANNOUNCE_COOLDOWN_CHANGES { get; set; } = false; // Should the bot announce when a command goes on cooldown in the debug channel? Defaults to false because it's spammy. Useful for begugging commands that aren't working.

        public void saveConfig()
        {
            File.WriteAllText("config/config.cfg", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public string getProperty(string property)
        {
            BotConfig cfg = Program.Instance._config;
            PropertyInfo prop = cfg.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
                return prop.GetValue(cfg).ToString();
            else
                return String.Format("Property '{0}' doesn't exist in the config.", property);
        }

        public void setProperty(string property, object value)
        {
            if (property.Equals("BOT_OWNERS"))
                throw new InvalidUpdateMethodException("This config value should be updated with add/remove-owner");
            try {
                BotConfig cfg = Program.Instance._config;
                PropertyInfo prop = cfg.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(cfg, Convert.ChangeType(value, prop.PropertyType));
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(String.Format("Could not update config property '{0}'", property), e);
            }
        }

    }
}
