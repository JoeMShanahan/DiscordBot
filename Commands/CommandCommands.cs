using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Utilities;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class CommandCommands : CommandBase, ICommand
    {
        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            string init = e.Message.Text.FromNthDeliminator(0, ' ');
            string @char = init.Substring(0, 1);
            string botname = Program.Instance.client.CurrentUser.Name;
            string botmention = "@" + botname;
            try
            {
                string command = e.Message.Text.FromNthDeliminator(1, ' ');
                ICommand[] matchingCommands = CommandManager.Instance.getCommandsMatchingName(command);
                if (matchingCommands.Length == 0)
                {
                    e.Channel.SendMessageFormatted("Sorry, {0}, but I didn't find any commands matching '{1}'", e.User.Mention, command);
                }
                else
                {
                    StringBuilder sb = new StringBuilder(String.Format("The following commands were returned for the search '{0}'.\n", command));
                    foreach (ICommand c in matchingCommands.Where(a => a.getRequiredPermissionLevel() <= Utils.getCommandPermissionlevelForUser(e.User, e.Server)))
                    {
                        bool phrase = c.GetType().IsSubclassOf(typeof(PhraseCommandBase)) || c.GetType().IsSubclassOf(typeof(PhraseCommandSimpleReplyBase));
                        string trigger = phrase ? "`"+c.triggerPattern().Replace("%me%", String.Format("({0}|{1})", botname, botmention))+"`" : c.usageText().Replace("%c%", String.Format("{0}{1}", phrase ? "" : @char, c.CommandName));
                        string line1 = String.Format("\t**{2}{0}**{3}: {1}", c.CommandName, trigger, phrase ? "" : @char, phrase ? " [Phrase]" : "");
                        string line2 = String.Format("\t\t{0}", c.helpText());
                        string line3 = "";
                        if (c.getCommandAliases().Length > 0)
                            line3 = String.Format("**Aliases**: {0}", c.getCommandAliases().Join(", "));
                        if (sb.ToString().Length + line1.Length + line2.Length + line3.Length > 2000)
                        {
                            sendMessage(sb.ToString(), pub, e);
                            sb = new StringBuilder();
                        }
                        sb.AppendLine(line1);
                        sb.AppendLine(line2);
                        sb.AppendLine(line3);
                    }
                    //sb.AppendLine("_All commands are case-insensitive. Parameters surrounded by angled brackets (< and >) are **required**. Ones surrounded by square brackets ([ and ]) are **optional**. Phrase commands will display their **regex based triggers**._");
                    sendMessage(sb.ToString(), pub, e);
                }
            }
            catch (IndexOutOfRangeException)
            {
                StringBuilder sb = new StringBuilder(String.Format("{0}, here are a list of commands you have access to -  Use `{1} <command name>` for more information on a command.\n", e.User.Mention, init));
                foreach (ICommand c in CommandManager.Instance.getCommandList().Where(a => a.getRequiredPermissionLevel() <= Utils.getCommandPermissionlevelForUser(e.User, e.Server)))
                {
                    bool phrase = c.GetType().IsSubclassOf(typeof(PhraseCommandBase)) || c.GetType().IsSubclassOf(typeof(PhraseCommandSimpleReplyBase));
                    string trigger = phrase ? "`" + c.triggerPattern().Replace("%me%", String.Format("({0}|{1})", botname, botmention)) + "`" : c.usageText().Replace("%c%", String.Format("{0}{1}", phrase ? "" : @char, c.CommandName));
                    string line = String.Format("\t**{0}{1}**{3}: {2}", phrase ? "" : @char, c.CommandName, trigger, phrase ? " [Phrase]" : "");
                    if (sb.ToString().Length + line.Length > 2000)
                    {
                        sendMessage(sb.ToString(), pub, e);
                        sb = new StringBuilder();
                    }
                    sb.AppendLine(line);
                }
                sb.AppendLine("_All commands are case-insensitive. Parameters surrounded by angled brackets (< and >) are **required**. Ones surrounded by square brackets ([ and ]) are **optional**. Phrase commands will display their **regex based triggers**._");
                sendMessage(sb.ToString(), pub, e);
            }
        }

        private void sendMessage(string message, bool pub, MessageEventArgs e)
        {
            if (pub)
                e.Channel.SendMessage(message);
            else
                e.User.SendMessage(message);
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "help", "cmds", "commandlist"/*, "listcommands"*/, "cmdlist", "listcmds" };
        }

        public override string helpText()
        {
            return "Returns a list of commands, or, if specified, details on the specified command";
        }

        public override string usageText()
        {
            return "%c% [command]";
        }
    }
}
