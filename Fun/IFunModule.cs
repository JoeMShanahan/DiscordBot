using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Fun
{
    public interface IFunModule
    {

        void onMessageReceived(MessageEventArgs e);
        void onMessageUpdated(MessageUpdatedEventArgs e);
        void onDMReceived(MessageEventArgs e);
        void onServerJoined(ServerEventArgs e);
        void onServerLeft(ServerEventArgs e);
        void onUserUpdate(UserUpdatedEventArgs e);
        void onUserLeft(UserEventArgs e);
        void onUserJoined(UserEventArgs e);

    }
}
