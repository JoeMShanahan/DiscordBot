using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using DiscordBot.Logging;

namespace DiscordBot.Commands
{
    public class CommandIgnore : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase)
        {

            try
            {
                ulong userID = Convert.ToUInt64(e.Message.Text.Split(' ')[1]);

                if (Program.Instance._config.botOwnerAccountIDs.Contains(userID))
                {
                    e.Channel.SendMessage(String.Format("I'm afraid I can't let you do that, {0}. UserID {1} belongs to a bot owner and therefore cannot be ignored.", e.User.Mention, userID));
                    string msgStr = String.Format("User '{0}' [{1}] attempted to set ignore flag on a bot owner", e.User.Name, e.User.Id);
                    Program.Instance.Logger.Log(msgStr, LogLevel.WARNING);
                    Utils.sendToDebugChannel(msgStr);
                    return;
                }

                ulong[] ignored = Program.Instance._config.ignoredUsers;
                ulong[] @new = new ulong[ignored.Length + 1];

                int x = 0;
                for (; x < ignored.Length; x++)
                    @new[x] = ignored[x];
                @new[x] = userID;

                Program.Instance._config.ignoredUsers = @new;
                Program.Instance._config.saveConfig();

                Utils.sendToDebugChannel("Ignoring commands from user ID {0}", userID);
            }
            catch (IndexOutOfRangeException)
            {
                e.Channel.SendMessage("No user ID specified.");
            }
            catch (Exception _e)
            {
                e.Channel.SendMessage(String.Format("Couldn't ignore user: {0} // {1}", _e.ToString(), _e.Message));
            }

        }

        public override string[] getCommandAliases()
        {
            return new string[] { "ignoreuser" };
        }

        public override bool goesOnCooldown()
        {
            return false;
        }
    }
}
