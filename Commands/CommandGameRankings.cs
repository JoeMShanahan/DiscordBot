using Discord;
using DiscordBot.Extensions;
using DiscordBot.Fun;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    // Thanks to Gav for this idea, he's awesome - even if he does say so himself.
    public class CommandGameRankings : CommandBase, ICommand
    {
        public override string helpText()
        {
            return "Displays information how many people are playing specific games accross all servers.";
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "gameranks", "gr", "playercount", "playercounts", "gameranking" };
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            int gameCount = 5;
            try
            {
                gameCount = Convert.ToInt32(e.Message.Text.FromNthDeliminator(1, ' '));
            }
            catch { }

            if (gameCount > 20) { gameCount = 20; }

            Dictionary<string, int> gameList = new Dictionary<string, int>();
            List<ulong> checkedUsers = new List<ulong>();
            Server[] servers = Program.Instance.client.Servers.ToArray();

            FunModuleRandomGame fmrg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            int ignoredCount = 0;
            int userCount = 0;
            foreach (Server s in servers)
            {
                userCount += s.UserCount;
                foreach (User u in s.Users)
                {
                    bool ignored = checkedUsers.Contains(u.Id) || u.Id == Program.Instance.client.CurrentUser.Id || u.IsBot || Utils.isUserIgnored(u.Id) || !u.CurrentGame.HasValue || u.CurrentGame == null || /*(*/u.CurrentGame.Value.Url != null/* || u.CurrentGame.Value.Url != string.Empty)*/ || fmrg.ignoredEntities.Count(a => a.Type == FunModuleRandomGame.GameIgnoreEntry.IgnoreType.USER && a.UniqueID == u.Id) > 0; // Crikey mate
                    checkedUsers.Add(u.Id);
                    //FunManager.Instance.Logger.Log("USER: {0}, GAME: {1}, IGNORED: {2}", u.Name, u.CurrentGame.HasValue ? u.CurrentGame.Value.Name : "-", ignored);
                    if (ignored) { 
                        //FunManager.Instance.Logger.Log("USER IS IGNORED");
                        ignoredCount++;
                        continue;
                    }
                    string game = u.CurrentGame.Value.Name;
                    if (fmrg._blacklistedGames.Contains(game))
                        continue;
                    if (gameList.Keys.Contains(game))
                        gameList[game]++;
                    else
                        gameList.Add(game, 1);

                }
            }

            StringBuilder sb = new StringBuilder();

            if (gameList.Count == 0) // Let's be honest, the only time this will happen is during testing.
                sb.Append("Nobody is playing any games right now!");
            else
            {
                sb.AppendFormattedLine("Top {2} currently played games, based on **{0}** servers and **{1}** users, of which **{3}** are playing games:", servers.Count(), userCount, gameList.Count < gameCount ? gameList.Count : gameCount, userCount - ignoredCount);
                IOrderedEnumerable<KeyValuePair<string, int>> sorted = gameList.OrderByDescending(k => k.Value);
                int x = 1;
                foreach (KeyValuePair<String, int> kvp in sorted)
                {
                    sb.AppendFormattedLine("\t**{0}**. {1}: {2}", x, kvp.Key, kvp.Value);
                    if (x++ == gameCount)
                        break;
                }
            }

            e.Channel.SendMessage(sb.ToString());


        }
    }

}
