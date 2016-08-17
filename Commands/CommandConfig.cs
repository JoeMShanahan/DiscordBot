using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;

namespace DiscordBot.Commands
{
    public class CommandConfig : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase)
        {
            string command = e.Message.Text.Split(' ')[1].ToLower();
            string reply = "";

            if (command.Equals("save")) {
                Program.Instance._config.saveConfig();
            }
            else if (command.Equals("no_admin_spam"))
            {
                Program.Instance._config.enableAdminAntiSpam = Convert.ToBoolean(e.Message.Text.Split(' ')[2]);
            }
            /*else if (command.Equals("set")) {
                string configParam = e.Message.Text.Split(' ')[2];
                int start = configParam.Length + e.Message.Text.IndexOf(configParam) + 1;
                string newValue = e.Message.Text.Substring(start);
                try {
                    Program.Instance._config.setProperty(configParam, newValue);
                    reply = String.Format("{0}, the config parameter '{1}' has been updated to the value '{2}'", e.User.Mention, configParam, newValue);
                }
                catch (Exception _e) { reply = "Couldn't update config property: " + _e.Message; }
            }*/
            else {
                string param = e.Message.Text.Split(' ')[1];
                reply = Program.Instance._config.getProperty(param);
            }

            if (pub) e.Channel.SendMessage(reply);
            else e.User.SendMessage(reply);

        }

        public override bool goesOnCooldown()
        {
            return false;
        }
    }
}
