using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Fun
{
    public class FunModuleBase : IFunModule
    {
        public virtual void onDMReceived(MessageEventArgs e) { }
        public virtual void onMessageReceived(MessageEventArgs e) { }
        public virtual void onMessageUpdated(MessageUpdatedEventArgs e) { }
        public virtual void onServerJoined(ServerEventArgs e) { }
        public virtual void onServerLeft(ServerEventArgs e) { }
        public virtual void onUserJoined(UserEventArgs e) { }
        public virtual void onUserLeft(UserEventArgs e) { }
        public virtual void onUserUpdate(UserUpdatedEventArgs e) { }
    }
}
