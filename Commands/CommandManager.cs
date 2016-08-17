using Discord;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public enum CommandPermissionLevel
    {
        NORMAL_USER = 0,
        SERVER_ADMIN = 1,
        SERVER_OWNER = 2,
        BOT_OWNER = 3,
        TEST = 4 // Nobody has this level. It's for testing invalid permissions!
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
        public Dictionary<User, Dictionary<ICommand, DateTime>> _userCooldowns = new Dictionary<User, Dictionary<ICommand, DateTime>>();

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

            // This is a mess now

            if (e.Server == null)
            {
                if (!_userCooldowns.ContainsKey(e.User)) { return false; } // No sever entry, no command are on cooldown for that server
                else if (!_userCooldowns[e.User].ContainsKey(c)) { return false; } // Command is not in the cooldown list
                else
                    return DateTime.Compare((_userCooldowns[e.User][c].AddSeconds(c.cooldownLength())), DateTime.Now) == 1;
            }

            // It should never reach here if it's a PM (famous last words)
            if (Utils.userIsServerOwner(e.User, e.Server) && !Program.Instance._config.enableAdminAntiSpam) { return false; } // User is the owner of the server and we're not preventing admin spam, command is available.
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

        public void putCommandOnCooldown(ICommand c, User u)
        {

            if (!c.goesOnCooldown()) { return; }

            if (_userCooldowns.ContainsKey(u))
            {
                if (_userCooldowns[u].ContainsKey(c))
                {
                    _userCooldowns[u][c] = DateTime.Now;
                }
                else
                {
                    _userCooldowns[u].Add(c, DateTime.Now);
                }
            }
            else _userCooldowns.Add(u, new Dictionary<ICommand, DateTime> { { c, DateTime.Now } });
            if (Program.Instance._config.debugAnnounceCooldownChanges && Program.Instance._config.useDebugChannel)
                Utils.sendToDebugChannel("Command '{0}' is now on cooldown for user '{1}' [{2}]", c.ToString(), u.Name, u.Id);
        }


        public void manageCommandInvokes(ICommand[] commandsToInvoke, MessageEventArgs e, bool forcePrivate = false, bool isPhrase = false)
        {
            foreach (ICommand c in commandsToInvoke)
            {
                bool isPublic = forcePrivate ? false : Program.Instance._config.publicRepyCommandCharacters.Contains(e.Message.Text.Substring(0, 1)) || isPhrase;

                string logString = string.Empty;
                if (isPublic)
                {
                    logString = String.Format("Invoking command '{0}' for user '{1}' [{2}] on server '{3}' [{4}]", c.ToString(), e.User.Name, e.User.Id, e.Server.Name, e.Server.Id);
                    Program.Instance.serverLogManager.getLoggerForServer(e.Server).Log(e.Channel, logString);
                }
                else
                {
                    logString = String.Format("Invoking command '{0}' for user '{1}' [{2}]", c.ToString(), e.User.Name, e.User.Id);
                    Program.Instance.messageLogger.Log(e.Channel, logString);
                }
                this.Logger.Log(logString);

                if (IsCommandOnCooldown(c, e)) { continue; }
                CommandPermissionLevel user_perms = Utils.getCommandPermissionlevelForUser(e.User, e.Server);
                if (c.getRequiredPermissionLevel() > user_perms) {
                    string permErr = string.Empty;
                    if (isPublic)
                        permErr = String.Format("User '{0}' [{6}] attempted to use command '{1}' and failed a permissions check on server '{2}' [{3}] // {4} (req.) vs {5} (has)", e.User.Name, c.ToString(), e.Server.Name, e.Server.Id, c.getRequiredPermissionLevel(), user_perms, e.User.Id);
                    else
                        permErr = String.Format("User '{0}' [{4}] attempted to use command '{1}' and failed a permissions check // {2} (req.) vs {3} (has)", e.User.Name, c.ToString(), c.getRequiredPermissionLevel(), user_perms, e.User.Id);
                    this.Logger.Log(permErr, LogLevel.WARNING);
                    if (Program.Instance._config.useDebugChannel && Program.Instance._config.announcePermissionErrors)
                    {
                        Utils.sendToDebugChannel(permErr);
                    }
                    continue;
                }
                c.invoke(e, isPublic);

                /*if (forcePrivate) // Hacky way of making DM replies appear in the log for the user who sent the initial command
                    Program.Instance.messageLogger.Log(e.User, "{0} [{1}]: {2}", Program.Instance.client.CurrentUser.Name, Program.Instance.client.CurrentUser.Name, e.Message.Text);*/
                if (isPublic)
                    putCommandOnCooldown(c, e.Server);
                else
                    putCommandOnCooldown(c, e.User);
            }
        }

        public void invokeCommandsFromName(string name, MessageEventArgs e, bool forcePrivate = false)
        {
            if (Utils.isUserIgnored(name, e)) { return; }
            List<ICommand> _toInvoke = _commands.FindAll(c => c.ToString().ToLower().Substring(c.ToString().LastIndexOf(".") + 8).Equals(name.ToLower()) || c.getCommandAliases().Contains(name.ToLower())); // All command clases start with "Command", so we have to make sure we strip that when checking for matching command names
            manageCommandInvokes(_toInvoke.ToArray(), e, forcePrivate);
        }

        public void invokeMatchingPhraseCommands(MessageEventArgs e)
        {
            if (Utils.isUserIgnored("PHRASE_COMMAND", e)) { return; }
            List<ICommand> _toinvoke = _commands.FindAll(c => !c.triggerPattern().Equals(string.Empty) && Regex.IsMatch(e.Message.RawText, c.triggerPattern().Replace("%me%", String.Format("<@{0}>", Program.Instance.client.CurrentUser.Id)), RegexOptions.IgnoreCase));
            manageCommandInvokes(_toinvoke.ToArray(), e, false, true);
        }

        public void loadCommandClasses()
        {
            this.Logger.Log("Attempting to initialise commands");
            if (this._commands.Count > 0)
                this._commands.Clear();
            string[] blacklistedCommandClasses = new string[] { "CommandManager", "CommandBase", "CommandPermissionLevel", "CommandSimpleReplyBase", "PhraseCommandBase" };
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            Type[] _toInit = types.Where(a => (a.Name.StartsWith("Command") || a.Name.StartsWith("PhraseCommand")) && !blacklistedCommandClasses.Contains(a.Name)).ToArray();
            this.Logger.Log("{0} command(s) to initialise", _toInit.Length);
            foreach (Type t in _toInit)
            {
                this.Logger.Log("Initialising command '{0}'", t.FullName);
                ICommand c = (ICommand)Activator.CreateInstance(t);
                this._commands.Add(c);
            }
        }

    }

}
