using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot.Commands
{
    public class CommandSetGame : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_OWNER;
        }

        public override void invoke(MessageEventArgs e, bool pub)
        {
            //Program.Instance.client.SetGame(new Game("Launch Schedule Simulator", GameType.Twitch, "https://ipeer.auron.co.uk/launchschedule/"));
            try
            {
                string newGame = e.Message.Text.Substring(e.Message.Text.Split(' ')[0].Length + 1);
                Program.Instance.client.SetGame(newGame);
            }
            catch (ArgumentOutOfRangeException)
            {
                Program.Instance.client.SetGame(null);
            }
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "game", "changegame" };
        }

        public override bool goesOnCooldown()
        {
            return false;
        }
    }
}
