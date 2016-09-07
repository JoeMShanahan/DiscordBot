using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using DiscordBot.Extensions;
using DiscordBot.Utilities;

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
            sb.AppendFormattedLine("Version: **{0}**", Utils.getBotVersion());
            sb.AppendFormattedLine("Uptime (d:h:m:s): **{0}**, MEMORY - Private: **{1}**, Virtual: **{2}**, GC: **{3}**", Utils.FormatUptime(Program.Instance.getUptime()), Utils.FormatBytes(memory), Utils.FormatBytes(vMemory), Utils.FormatBytes(gcMemory));
            sb.AppendFormattedLine("Commands parsed: **{0}** | SERVERS - Count: **{1}**, Channels: **{2}** (**{3}** text, **{4}** voice), Users: **{5}**", commandsTriggered + phraseCommandsTriggered, servers, textChannels + voiceChannels, textChannels, voiceChannels, users);

            e.Channel.SendMessage(sb.ToString());

        }

        public override string[] getCommandAliases()
        {
            return new string[] { "debug", "status", "info" };
        }
    }
}
