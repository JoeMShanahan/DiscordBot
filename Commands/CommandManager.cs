using Discord;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public enum CommandPermissionLevel
    {
        NORMAL_USER = 0,
        SERVER_ADMIN = 1,
        SERVER_OWNER = 2,
        BOT_OWNER = 3
    }

    public class CommandManager
    {

        public List<ICommand> _commands = new List<ICommand>
        {
            new CommandAmIOwner(),
            new CommandListServers(),
            new CommandMyPermissions(),
            new CommandConfig(),
            new CommandQuit(),
            new CommandNextLaunch(),
            new CommandLeave(),
            new CommandTest(),
            new CommandLastLaunch()
        };
        public Dictionary<Server, Dictionary<ICommand, DateTime>> _cooldowns = new Dictionary<Server, Dictionary<ICommand, DateTime>>();

        public static CommandManager Instance { get; private set; }

        public CommandManager()
        {
            Instance = this;
        }

        public bool IsCommandOnCooldown(ICommand c, MessageEventArgs e)
        {
            if (Utils.userIsOwner(e.User)/* && !Program.Instance._config.NO_ADMIN_SPAM*/) { return false; } // Command are always avaialble to owners :)
            else if (Utils.userIsServerOwner(e.User, e.Server) && !Program.Instance._config.NO_ADMIN_SPAM) { return false; } // User is the owner of the server and we're not preventing admin spam, command is available.
            else if (Utils.userIsServerAdmin(e.User) && !Program.Instance._config.NO_ADMIN_SPAM) { return false; } // User is admin on the server and we're not preventing admin spam, therefore, command is available.
            else if (!_cooldowns.ContainsKey(e.Server)) { return false; } // No sever entry, no command are on cooldown for that server
            else if (!_cooldowns[e.Server].ContainsKey(c)) { return false; } // Command is not in the cooldown list
            else {
                //Utils.sendToDebugChannel("{0} // {1} // {2}", _cooldowns[e.Server][c].AddSeconds(Program.Instance._config.COMMAND_COOLDOWN_SECS), DateTime.Now, DateTime.Compare((_cooldowns[e.Server][c].AddSeconds(Program.Instance._config.COMMAND_COOLDOWN_SECS)), DateTime.Now));
                return DateTime.Compare((_cooldowns[e.Server][c].AddSeconds(Program.Instance._config.COMMAND_COOLDOWN_SECS)), DateTime.Now) == 1; }
        }

        public void putCommandOnCooldown(ICommand c, Server s)
        {
            if (_cooldowns.ContainsKey(s)) {
                if (_cooldowns[s].ContainsKey(c))
                {
                    _cooldowns[s][c] = DateTime.Now;
                }
                else
                {
                    _cooldowns[s].Add(c, DateTime.Now);
                }
            }
            else _cooldowns.Add(s, new Dictionary<ICommand, DateTime>{ { c, DateTime.Now } });
            if (Program.Instance._config.ANNOUNCE_COOLDOWN_CHANGES && Program.Instance._config.USE_DEBUG_CHANNEL)
                Utils.sendToDebugChannel("Command '{0}' is now on cooldown for server '{1}' [{2}]", c.ToString(), s.Name, s.Id);
        }

        public void manageCommandInvokes(ICommand[] commandsToInvoke, MessageEventArgs e)
        {
            foreach (ICommand c in commandsToInvoke)
            {
                if (IsCommandOnCooldown(c, e)) { continue; }
                CommandPermissionLevel user_perms = Utils.getCommandPermissionlevelForUser(e.User, e.Server);
                if (c.getRequiredPermissionLevel() > user_perms) {
                    if (Program.Instance._config.USE_DEBUG_CHANNEL && Program.Instance._config.LOG_PERMISSION_ERRORS)
                        Utils.sendToDebugChannel("User '{0}' [{6}] attempted to use command '{1}' and failed a permissions check on server '{2}' [{3}] // {4} (req.) vs {5} (has)", e.User.Name, c.ToString(), e.Server.Name, e.Server.Id, c.getRequiredPermissionLevel(), user_perms, e.User.Id);
                    continue;
                }
                bool isPublic = Program.Instance._config.PUBLIC_COMMAND_CHARACTERS.Contains(e.Message.Text.Substring(0, 1));
                c.invoke(e, isPublic);
                putCommandOnCooldown(c, e.Server);
            }
        }

        public void invokeCommandsFromName(string name, MessageEventArgs e)
        {
            List<ICommand> _toInvoke = _commands.FindAll(c => c.ToString().ToLower().Substring(c.ToString().LastIndexOf(".") + 8).Equals(name.ToLower())); // All command clases start with "Command", so we have to make sure we strip that when checking for matching command names
            manageCommandInvokes(_toInvoke.ToArray(), e);
        }

    }

}
