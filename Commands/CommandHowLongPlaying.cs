using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Fun;
using DiscordBot.Utilities;
using DiscordBot.Extensions;

namespace DiscordBot.Commands
{
    public class CommandHowLongPlaying : CommandBase, ICommand
    {

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            try
            {
                FunModuleRandomGame rg = (FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame");
                if (rg.isPlayingGame)
                {
                    DateTime s = rg.startedPlaying;
                    string game = rg.currentGame;
                    string currentPlayTime = Utils.FormatUptime((DateTime.Now - s));
                    e.Channel.SendMessageFormatted("{0}, I have been playing **{1}** for **{2}** (d:h:m:s) (**{3}** all time)", e.User.Mention, game, currentPlayTime, (rg._playTimes.ContainsKey(rg.currentGame) ? Utils.FormatUptime(TimeSpan.FromSeconds(rg._playTimes[rg.currentGame])) : currentPlayTime));
                }
                else
                {
                    e.Channel.SendMessageFormatted("{0}, I'm not currently playing a game!", e.User.Mention);
                }

            } catch
            {
                e.Channel.SendMessage("Couldn't query current game data because the module was not found.");
            }
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "hlp", "howlong", "beenplaying", "gametime" };
        }

        public override string triggerPattern()
        {
            if (((FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame")).isPlayingGame)
            {
                return String.Format(@"%me%,? how long have you been playing (that( game)?|{0})\??", System.Text.RegularExpressions.Regex.Escape(((FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame")).currentGame));
            }
            else
            {
                return @"%me%,? how long have you been playing that( game)?\??";
            }
        }
    }
}
