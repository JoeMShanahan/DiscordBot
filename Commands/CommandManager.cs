using Discord;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public Logger Logger { get; private set; }

        public List<ICommand> _commands = new List<ICommand>();
        /*{
            new CommandAmIOwner(),
            new CommandListServers(),
            new CommandMyPermissions(),
            new CommandConfig(),
            new CommandQuit(),
            new CommandNextLaunch(),
            new CommandLeave(),
            new CommandTest(),
            new CommandLastLaunch(),
            new CommandLaunches(),
            new CommandSuperStrypi(),
            new CommandListCommands()
        };*/
        public Dictionary<Server, Dictionary<ICommand, DateTime>> _cooldowns = new Dictionary<Server, Dictionary<ICommand, DateTime>>();

        public static CommandManager Instance { get; private set; }

        public CommandManager()
        {
            this.Logger = new Logger("CommandManager");
            Instance = this;
            this.loadCommandClasses();
        }

        public bool IsCommandOnCooldown(ICommand c, MessageEventArgs e)
        {
            if (Utils.userIsOwner(e.User)/* && !Program.Instance._config.NO_ADMIN_SPAM*/) { return false; } // Command are always avaialble to owners :)
            else if (Utils.userIsServerOwner(e.User, e.Server) && !Program.Instance._config.enableAdminAntiSpam) { return false; } // User is the owner of the server and we're not preventing admin spam, command is available.
            else if (Utils.userIsServerAdmin(e.User) && !Program.Instance._config.enableAdminAntiSpam) { return false; } // User is admin on the server and we're not preventing admin spam, therefore, command is available.
            else if (!_cooldowns.ContainsKey(e.Server)) { return false; } // No sever entry, no command are on cooldown for that server
            else if (!_cooldowns[e.Server].ContainsKey(c)) { return false; } // Command is not in the cooldown list
            else
                return DateTime.Compare((_cooldowns[e.Server][c].AddSeconds(c.cooldownLength())), DateTime.Now) == 1;
        }

        public void putCommandOnCooldown(ICommand c, Server s)
        {

            if (!c.goesOnCooldown()) { return; }

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
            if (Program.Instance._config.debugAnnounceCooldownChanges && Program.Instance._config.useDebugChannel)
                Utils.sendToDebugChannel("Command '{0}' is now on cooldown for server '{1}' [{2}]", c.ToString(), s.Name, s.Id);
        }

        public void manageCommandInvokes(ICommand[] commandsToInvoke, MessageEventArgs e)
        {
            foreach (ICommand c in commandsToInvoke)
            {
                this.Logger.Log("Invoking command '{0}' for user '{1}' [{2}] on server '{3}' [{4}]", c.ToString(), e.User.Name, e.User.Id, e.Server.Name, e.Server.Id);
                if (IsCommandOnCooldown(c, e)) { continue; }
                CommandPermissionLevel user_perms = Utils.getCommandPermissionlevelForUser(e.User, e.Server);
                if (c.getRequiredPermissionLevel() > user_perms) {
                    string permErr = String.Format("User '{0}' [{6}] attempted to use command '{1}' and failed a permissions check on server '{2}' [{3}] // {4} (req.) vs {5} (has)", e.User.Name, c.ToString(), e.Server.Name, e.Server.Id, c.getRequiredPermissionLevel(), user_perms, e.User.Id);
                    this.Logger.Log(permErr, LogLevel.WARNING);
                    if (Program.Instance._config.useDebugChannel && Program.Instance._config.announcePermissionErrors)
                    {
                        Utils.sendToDebugChannel(permErr);
                    }
                    continue;
                }
                bool isPublic = Program.Instance._config.publicRepyCommandCharacters.Contains(e.Message.Text.Substring(0, 1));
                c.invoke(e, isPublic);
                putCommandOnCooldown(c, e.Server);
            }
        }

        public void invokeCommandsFromName(string name, MessageEventArgs e)
        {
            List<ICommand> _toInvoke = _commands.FindAll(c => c.ToString().ToLower().Substring(c.ToString().LastIndexOf(".") + 8).Equals(name.ToLower()) || c.getCommandAliases().Contains(name.ToLower())); // All command clases start with "Command", so we have to make sure we strip that when checking for matching command names
            manageCommandInvokes(_toInvoke.ToArray(), e);
        }

        public void loadCommandClasses()
        {
            this.Logger.Log("Attempting to initialise commands");
            if (this._commands.Count > 0)
                this._commands.Clear();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] _toInit = types.Where(a => a.Name.StartsWith("Command") && !a.Name.Equals("CommandBase")).ToArray();
            this.Logger.Log("{0} command(s) to initialise", _toInit.Length);
            string[] blacklistedCommandClasses = new string[] { "CommandManager", "CommandBase", "CommandPermissionLevel" };
            foreach (Type t in types.Where(a => a.Name.StartsWith("Command") && !blacklistedCommandClasses.Contains(a.Name)))
            {
                this.Logger.Log("Initialising command '{0}'", t.FullName);
                ICommand c = (ICommand)Activator.CreateInstance(t);
                this._commands.Add(c);
            }
        }

    }

}
