using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Extensions
{
    public static class DiscordNETExtensions
    {
        /* CHANNEL */ 

        public static Task<Message> SendMessageFormatted(this Channel c, string message, params object[] fillers) => c.SendMessage(String.Format(message, fillers));

        /* USER */

        public static Task<Message> SendMessageFormatted(this User c, string message, params object[] fillers) => c.SendMessage(String.Format(message, fillers));

    }
}
