using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using DiscordBot.Extensions;
using DiscordBot.Utilities;
using System.Reflection;

namespace DiscordBot.Commands
{
    public class CommandStats : CommandBase, ICommand
    {
        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {

            long memory = 0L;
            long vMemory = 0L;
            long gcMemory = GC.GetTotalMemory(true);

            long commandsTriggered = Program.Instance.commandManager.commandsTriggered;
            long phraseCommandsTriggered = Program.Instance.commandManager.phraseCommandsTriggered;

            int servers = Program.Instance.client.Servers.Count();
            int users = 0;
            int voiceChannels = 0;
            int textChannels = 0;

            foreach (Server s in Program.Instance.client.Servers)
            {
                users += s.UserCount;
                voiceChannels += s.VoiceChannels.Count();
                textChannels += s.TextChannels.Count();
            }

            using (Process me = Process.GetCurrentProcess())
            {
                memory = me.PrivateMemorySize64;
                vMemory = me.VirtualMemorySize64;
            }

            StringBuilder sb = new StringBuilder();
            string buildTime = Utils.getAssemblyBuildTime();
            sb.AppendFormattedLine("VERSION: **{0}** built on **{1}**", Utils.getBotVersion(), buildTime);
            sb.AppendFormattedLine("UPTIME (d:h:m:s): **{0}**", Utils.FormatUptime(Program.Instance.getUptime()));
            sb.AppendFormattedLine("MEMORY (P/V/GC): **{0}**/**{1}**/**{2}**", Utils.FormatBytes(memory), Utils.FormatBytes(vMemory), Utils.FormatBytes(gcMemory));
            sb.AppendFormattedLine("Commands parsed: **{0}** | SERVERS - Count: **{1}**, Channels: **{2}** (**{3}** text, **{4}** voice), Users: **{5}**", commandsTriggered + phraseCommandsTriggered, servers, textChannels + voiceChannels, textChannels, voiceChannels, users);

            e.Channel.SendMessage(sb.ToString());

        }

        public override string[] getCommandAliases()
        {
            return new string[] { "debug", "status", "info" };
        }

        public override string helpText()
        {
            return "Displays (mostly debug) information on the bot's current session";
        }
    }
}
