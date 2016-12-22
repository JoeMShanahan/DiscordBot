using Discord;
using DiscordBot.Commands;
using DiscordBot.Extensions;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static DiscordBot.Fun.FunModuleRandomGame;

namespace DiscordBot.Fun
{

    public class CommandBlacklistGame : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string helpText()
        {
            return "Blacklists games to stop LaunchBot from learning them.";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            string gameList = e.Message.Text.FromNthDeliminator(1, ' ');
            string[] games = gameList.SplitWithEscapes(',');

            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");

            foreach (string s in games)
            {
                if (rg.isPlayingGame && rg.currentGame.Equals(s))
                    rg.setRandomGame();
                if (rg._gameNames.Contains(s.Replace("\\,", ",")))
                    rg._gameNames.Remove(s.Replace("\\,", ","));
                if (!rg._blacklistedGames.Contains(s.Replace("\\,", ",")))
                    rg._blacklistedGames.Add(s.Replace("\\,", ","));
                if (rg._playTimes.ContainsKey(s.Replace("\\,", ",")))
                    rg._playTimes.Remove(s.Replace("\\,", ","));
                rg.saveGameBlacklist();
                rg.saveGameList();

            }
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "blg" };
        }

        public override string usageText()
        {
            return "%c% game1[,game2[,game3[,...]]]";
        }
    }

    public class CommandTop5Games : CommandBase, ICommand
    {
        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            bool top5 = e.Message.Text.Split(' ')[0].Substring(1).StartsWithIgnoreCase("top5");
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            Dictionary<string, long> games = new Dictionary<string, long>(rg._playTimes);
            if (rg.isPlayingGame)
            {
                if (games.ContainsKey(rg.currentGame))
                    games[rg.currentGame] += (long)(DateTime.Now - rg.startedPlaying).TotalSeconds;
                else
                    games.Add(rg.currentGame, (long)(DateTime.Now - rg.startedPlaying).TotalSeconds);
            }
            IOrderedEnumerable<KeyValuePair<string, long>> sorted = games.OrderByDescending(k => k.Value);
            int x = 0;
            StringBuilder sb = new StringBuilder(String.Format("Top {0} games, ordered by playtime:\n", top5 ? 5 : 10));
            foreach (KeyValuePair<string, long> kvp in sorted)
            {
                if (x++ >= (top5 ? 5 : 10)) { break; }
                long time = kvp.Value;
                //if (rg.isPlayingGame && rg.currentGame.EqualsIgnoreCase(kvp.Key))
                //    time += (long)(DateTime.Now - rg.startedPlaying).TotalSeconds; // I'm an idiot.
                sb.AppendFormattedLine("**{0}**. {1}: {2}", x, kvp.Key, Utils.FormatUptime(TimeSpan.FromSeconds(time)));
            }
            e.Channel.SendMessage(sb.ToString());
        }

        public override string[] getCommandAliases()
        {
            return new string[] { "top5", "top10", "top10games" };
        }

        public override string helpText()
        {
            return "Displays the top 5 or 10 games (depending on command used) games that LaunchBot has played";
        }

        public override string usageText()
        {
            return "%c%";
        }
    }

    public class CommandHowLongPlaying : CommandBase, ICommand
    {

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            try
            {
                FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
                if (rg.isPlayingGame)
                {
                    DateTime s = rg.startedPlaying;
                    string game = rg.currentGame;
                    string currentPlayTime = Utils.FormatUptime((DateTime.Now - s));
                    e.Channel.SendMessageFormatted("{0}, I have been playing **{1}** for **{2}** (d:h:m:s) (**{3}** all time)", e.User.Mention, game, currentPlayTime, (rg._playTimes.ContainsKey(rg.currentGame) ? Utils.FormatUptime(TimeSpan.FromSeconds(rg._playTimes[rg.currentGame] + (DateTime.Now - s).TotalSeconds)) : currentPlayTime));
                }
                else
                {
                    e.Channel.SendMessageFormatted("{0}, I'm not currently playing a game!", e.User.Mention);
                }

            }
            catch
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
            if ((FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame")).isPlayingGame)
            {
                return String.Format(@"%me%,? how long have you been playing (that( game)?|{0})\??", System.Text.RegularExpressions.Regex.Escape(((FunModuleRandomGame)FunManager.Instance.getFunModuleWithName("RandomGame")).currentGame));
            }
            else
            {
                return @"%me%,? how long have you been playing that( game)?\??";
            }
        }

        public override string helpText()
        {
            return "Displays how long the bot has been playing its current game (if any)";
        }

        public override string usageText()
        {
            return "%c%";
        }
    }

    public class CommandListGameTimes : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string helpText()
        {
            return "Displays **ALL** total game play times";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            if (rg._playTimes.Count() == 0)
            {
                e.Channel.SendMessage("No gameplay time data!");
            }
            else
            {
                StringBuilder b = new StringBuilder("```");
                foreach (string k in rg._playTimes.Keys)
                {
                    b.AppendFormat("{0}: {1}\n", k, Utils.FormatUpdate(rg._playTimes[k]));
                }
                b.Append("```");
                e.Channel.SendMessage(b.ToString());
            }
        }

        public override string usageText()
        {
            return "%c%";
        }
    }

    public class CommandListGames : CommandBase, ICommand
    {

        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string helpText()
        {
            return "Returns **ALL** games that the bot knows";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            StringBuilder sb = new StringBuilder("```");
            foreach (string s in rg._gameNames)
            {
                if (sb.ToString().Length + (s.Length + 3) > 2000) // Fix for games list exceeding 2,000 characters
                {
                    sb.Append("```");
                    e.Channel.SendMessageFormatted("{0}", sb.ToString());
                    sb.Clear();
                    sb.Append("```");
                }
                sb.AppendLine($"{s}");
            }
            e.Channel.SendMessageFormatted("{0}```", sb.ToString());
        }

        public override string usageText()
        {
            return "%c%";
        }
    }

    public class CommandIgnoreGamesFrom : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string helpText()
        {
            return "Adds the specified _userid_ to the game learning ignore list, stopping games they play from being learnt by the bot";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            ulong userid = 0;
            try
            {
                userid = Convert.ToUInt64(e.Message.Text.FromNthDeliminator(1, ' '));
                if (rg.ignoredEntities.Count(a => a.UniqueID == userid && a.Type == GameIgnoreEntry.IgnoreType.USER) == 0)
                {
                    rg.ignoredEntities.Add(new GameIgnoreEntry(userid, GameIgnoreEntry.IgnoreType.USER));
                    e.Channel.SendMessageFormatted("Added user ID {0} to game learn blacklist.", userid);
                    rg.saveIgnoredUsers();
                }
                else
                {
                    e.Channel.SendMessageFormatted("UserID {0} is already ignored!", userid);
                }
            }
            catch (Exception _e) { e.Channel.SendMessageFormatted("Unable to ignore user: {0}", _e.Message ?? "No Message"); }
        }

        public override string usageText()
        {
            return "%c% <userid>";
        }
    }

    public class CommandGameStats : CommandBase, ICommand
    {
        public override string helpText()
        {
            return "Displays information on the bot's current game database.";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame fmrg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            int gamecount = fmrg._gameNames.Count;
            int gamesplayed = fmrg._playTimes.Count;
            long totalplaytime = 0L;
            foreach (long u in fmrg._playTimes.Values)
                totalplaytime += u;
            e.Channel.SendMessageFormatted("{0}, I have **{1}** games in my database of which I have played **{2}** (**{3:P2}**). The games I have played total **{4}** (**d:h:m:s**) in playtime.", e.User.Mention, gamecount, gamesplayed, ((decimal)gamesplayed / (decimal)gamecount), Utils.FormatUptime(TimeSpan.FromSeconds(totalplaytime)));
        }

        public override string usageText()
        {
            return "%c%";
        }
    }

    public class CommandIgnoreGamesFromServer : CommandBase, ICommand
    {
        public override CommandPermissionLevel getRequiredPermissionLevel()
        {
            return CommandPermissionLevel.BOT_ADMIN;
        }

        public override string helpText()
        {
            return "Instructs the bot to ignore game changes for users on the server matching _serverid_";
        }

        public override void invoke(MessageEventArgs e, bool pub, bool fromPhrase = false)
        {
            FunModuleRandomGame rg = FunManager.Instance.getFunModuleWithName<FunModuleRandomGame>("RandomGame");
            ulong serverid = 0;
            try
            {
                serverid = Convert.ToUInt64(e.Message.Text.FromNthDeliminator(1, ' '));
                if (rg.ignoredEntities.Count(a => a.UniqueID == serverid && a.Type == GameIgnoreEntry.IgnoreType.SERVER) == 0)
                {
                    rg.ignoredEntities.Add(new GameIgnoreEntry(serverid, GameIgnoreEntry.IgnoreType.SERVER));
                    e.Channel.SendMessageFormatted("Added server ID {0} to game learn blacklist.", serverid);
                    rg.saveIgnoredUsers();
                }
                else
                {
                    e.Channel.SendMessageFormatted("ServerID {0} is already ignored!", serverid);
                }
            }
            catch (Exception _e) { e.Channel.SendMessageFormatted("Unable to ignore server: {0}", _e.Message ?? "No Message"); }
        }

        public override string usageText()
        {
            return "%c% <serverid>";
        }
    }

    public class FunModuleRandomGame : FunModuleBase, IFunModule
    {

        public class GameIgnoreEntry
        {
            public enum IgnoreType
            {
                USER,
                SERVER,
            }

            public ulong UniqueID { get; private set; }
            public IgnoreType Type { get; private set; }

            public GameIgnoreEntry(ulong uniqueID, IgnoreType type)
            {
                this.UniqueID = uniqueID;
                this.Type = type;
            }
        }

        public const int IGNORE_VERSION = 1;
        
        // These are mostly satire, but there's some legit games in here too
        // ^ let's be honest, they're mostly legit, with a few satire
        public List<string> _gameNames = new List<string> {
            "Kerbal Space Program",
            "Launch Schedule Simulator 2016",
            "Elite: Dangerous", // Steam says without colon, Discord says with.
            //"010100110111000001100001011000010110000101100001011000110110010100100001", <- not a game
            //"0011010000110010", // <- not a game
            "Universe Sandbox 2",
            "Euro Truck Simulator 2",
            "American Truck Simulator",
            "Portal 2",
            "The Waiting Game",
            "The Price Is Right",
            "No Man's Sky",
            "Half-Life 3", // How can you not have a random game easter egg that doesn't have HL3 in it?
            "with Fire",
            "Doctor",
            "Theme Hospital", // Hospital administrator is TOTALLY cheating
            "Bowling with Roman",
            "The Elder Scrolls V: Skyrim",
            "Rocket League",
            "Team Fortress 2",
            "in the woods",
            "in the rain",
            "with Regul[ea]r Expre(s){2}ions",
            "Pokémon Blue",
            "Pokémon Red",
            "Pokémon Yellow",
            "Minecraft",
            "Fallout Shelter",
            "Deus Ex: Mankind Divided",
            "Spot the Difference",
            "I Spy with My Little Eye",
            "Elite Dangerous: Horizons",
            "The Elder Scrolls IV: Oblivion",
            "Solitaire",
            "Freecell",
            "Poker",
            "Strip Poker",
            "Grand Theft Auto V",
            "Left 4 Dead 2",
            "Party Hard",
            "Starbound",
            "Terraria",
            "Factorio",
            "Prison Architect",
            "Super Amazing Wagon Adventure",
            "Farming Simulator 15",
            "Sid Meier's Civilization V"
        };

        public bool isPlayingGame = false;
#if DEBUG
        private int newGameMagicNumber = 75;
#else
        private int newGameMagicNumber = 1;
#endif
        private int newGameRollCeiling = 100; // 1% seemed too common, so I implemented this to enable "lower" percentages
        private DateTime _startedPlaying = new DateTime(1970, 1, 1);
        public DateTime startedPlaying {
            get
            {
#if DEBUG
                DateTime debug = this._startedPlaying.AddSeconds(-(new Random().RealNext(3600, 8400)));
                return debug;
#else
                return _startedPlaying;
#endif
            }
            private set
            {
                if (value != this._startedPlaying)
                    this._startedPlaying = value;
            }
        }
        private double _gamePlayingGraceMinutes = 30; // How many minutes should we "play" a game for minimum, even if the random chance is hit again?
        private Logger Logger;
        public string currentGame { get; private set; }
        public Dictionary<string, long> _playTimes = new Dictionary<string, long>();
        public List<string> _blacklistedGames = new List<string> { "Steam", "Unity", "Minecraft", "Visual Studio", "SCS Workshop Uploader", "skyrim", "Skyrim" };
        //public List<ulong> ignoredEntities = new List<ulong>();
        public List<GameIgnoreEntry> ignoredEntities = new List<GameIgnoreEntry>();

        public FunModuleRandomGame()
        {
            this.Logger = FunManager.Instance.Logger.createSubLogger("RandomGame");
            this.Logger.Log("Chance of playing or changing game is {0:P2} ({1}/{2})", (decimal)newGameMagicNumber / (decimal)newGameRollCeiling, newGameMagicNumber, newGameRollCeiling);
            this.Logger.Log("Attempting to load total gameplay times from config...");
            this.loadGamesTimes();
            this.loadGameList();
            this.loadGameBlacklist();
            this.loadIgnoredUsers();
        }

        public override void onUserUpdate(UserUpdatedEventArgs e)
        {
            try // There's an NRE here that I can't track down, so let's just ignore them for now, shall we? ;)
            {
                if (!e.After.IsBot && e.After.CurrentGame != null && e.After.CurrentGame.HasValue && !Utils.isUserIgnored(e.After.Id) && this.ignoredEntities.Count(a => (a.UniqueID == e.After.Id && a.Type == GameIgnoreEntry.IgnoreType.USER) || (a.UniqueID == e.After.Server.Id && a.Type == GameIgnoreEntry.IgnoreType.SERVER)) == 0 && !e.After.Name.Equals("LaunchBot") && !e.After.CurrentGame.Value.Name.Equals(string.Empty) && (e.After.CurrentGame.Value.Url == null || e.After.CurrentGame.Value.Url == string.Empty))
                {
                    string gameName = e.After.CurrentGame.Value.Name;
                    if (!this._blacklistedGames.Contains(gameName) && !gameName.ContainsIgnoreCase("Minecraft") && !this._gameNames.Contains(gameName))
                    {
                        this._gameNames.Add(gameName);
                        this.saveGameList();
                        this.Logger.Log("Learnt a new game: {0}", gameName);
                        Utils.sendToDebugChannel("[**RandomGame**] Learnt a new game from '{1}' [{2}]: {0}", gameName, e.After.Name, e.After.Id);
                    }
                }
            }
            catch (NullReferenceException) { }

            // 1.0.5.* adds the ability for the bot to change game on a user update event. 
            // This event's probability scales based on number of users, with a maximum chance of 1% and minimum of 0.1%
            double randomChance = (30 / Utils.getUserCountCached()) / 10; // 300 users = min value (0.1%); initial start value of 30%, but capped at 1%.
            // Because Random.NextDouble is weird and gives us a number from 0.0 to 1.0, these look a bit weird.
            if (randomChance < 0.001) // 0.1%
                randomChance = 0.001;
            if (randomChance > 0.01) // 1%
                randomChance = 0.01;

            if (new Random(Utils.getEpochTime()).NextDouble() <= randomChance) // You're winner!
            {
                this.handleGameRoll();
            }

        }

        public void loadGameList()
        {
            if (!File.Exists("config/FunModule_RandomGame_Games.cfg")) { return; }
            this._gameNames = new List<string>(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("config/FunModule_RandomGame_Games.cfg")));
            this.Logger.Log("Loaded {0} games from config", this._gameNames.Count);
        }

        public void saveGameList()
        {
            string json = JsonConvert.SerializeObject(this._gameNames, Formatting.Indented);
            File.WriteAllText("config/FunModule_RandomGame_Games.cfg", json);
        }

        public override void onServerJoined(ServerEventArgs e) // Restore game status after unplanned disconnects
        {
            if (this.isPlayingGame/* && !this.currentGame.Equals(Program.Instance.client.CurrentGame.Name)*/)
            {
                Program.Instance.client.SetGame(new Game(this.currentGame));
            }
        }

        public override void onMessageReceived(MessageEventArgs e)
        {

            Random r = new Random();
            int _newGame = r.RealNext(newGameRollCeiling);
            //this.Logger.Log("{0}", _newGame);
            if (_newGame <= newGameMagicNumber) // N% chance hit, pick a new game (or not)
            {

                this.handleGameRoll();

            }
        }

        public void handleGameRoll()
        {
            //this.Logger.Log("Random chance on GameChance was hit -- {0}", DateTime.Compare(this.startedPlaying.AddMinutes(_gamePlayingGraceMinutes), DateTime.Now));
            if (this.isPlayingGame && DateTime.Compare(this.startedPlaying.AddMinutes(_gamePlayingGraceMinutes), DateTime.Now) < 1)
            {
                int whatDo = (new Random(Utils.getEpochTime())).RealNext(1, 10); // If this is >= 5, we do nothing
                if (whatDo == 1) // 10%, stop "playing"
                {
                    this.saveCurrentGameTime();
                    this.Logger.Log("Stopped playing");
                    Program.Instance.client.SetGame(null);
                    this.isPlayingGame = false;
                }
                else if (whatDo >= 2 && whatDo < 5) // 40%, pick a new game
                {
                    this.saveCurrentGameTime();
                    setRandomGame();
                }
                // else do nothing
            }
            else if (!this.isPlayingGame)
            {
                this.isPlayingGame = true;
                setRandomGame();
            }
        }

        /*public override void onBotTerminating()
        {
            this.saveCurrentGameTime();
            //this.saveGameTimes();
        }*/

        public void setRandomGame()
        {
            this.startedPlaying = DateTime.Now;
            string game = _gameNames[new Random(Utils.getEpochTime()).Next(_gameNames.Count)];
            this.currentGame = game;
            this.Logger.Log("Now playing: {0}", game);
            Program.Instance.client.SetGame(new Game(game));
        }

        public void saveCurrentGameTime()
        {
            if (!this.isPlayingGame) { return; }
            Int64 secondsPlayed = (Int64)((TimeSpan)(DateTime.Now - this.startedPlaying)).TotalSeconds;
            if (this._playTimes.ContainsKey(this.currentGame))
                this._playTimes[this.currentGame] += secondsPlayed;
            else
                this._playTimes.Add(this.currentGame, secondsPlayed);
            this.saveGameTimes();
        }

        public void loadGamesTimes()
        {
            if (File.Exists("config/FunModule_RandomGame_Times.cfg"))
            {
                this._playTimes = new Dictionary<string, long>(JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText("config/FunModule_RandomGame_Times.cfg")));
                this.Logger.Log("Loaded {0} gameplay time entries from config file", this._playTimes.Count());
            }
            else
            {
                this.Logger.Log("Config file for gameplay times does not exist.");
            }
        }

        public void saveGameTimes()
        {
            string json = JsonConvert.SerializeObject(this._playTimes, Formatting.Indented);
            this.Logger.Log("Saving game play data: {0}", LogLevel.DEBUG, json);
            File.WriteAllText("config/FunModule_RandomGame_Times.cfg", json);
        }

        public void loadGameBlacklist()
        {
            if (!File.Exists("config/FunModule_RandomGame_Blacklist.cfg")) { return; }
            this._blacklistedGames = new List<string>(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("config/FunModule_RandomGame_Blacklist.cfg")));
            this.Logger.Log("Loaded {0} games from config", this._gameNames.Count);
        }

        public void saveGameBlacklist()
        {
            string json = JsonConvert.SerializeObject(this._blacklistedGames, Formatting.Indented);
            File.WriteAllText("config/FunModule_RandomGame_Blacklist.cfg", json);
        }

        public void loadIgnoredUsers()
        {
            if (!File.Exists("config/FunModule_RandomGame_Ignores.cfg")) { return; }
            //this.ignoredEntities = new List<ulong>(JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText("config/FunModule_RandomGame_Ignores.cfg")));
            int ignoreVersion = 0;
            string json = File.ReadAllText("config/FunModule_RandomGame_Ignores.cfg");
            JObject _json = new JObject();
            try
            {
                _json = JObject.Parse(json);
                try
                {
                    ignoreVersion = Convert.ToInt32(_json["IGNORE_VERSION"].ToString());
                }
                catch { }
            }
            catch { }
            this.Logger.Log("Current ignore version is {0}", ignoreVersion);
            if (ignoreVersion == 0)
            {
                this.Logger.Log("Applying IGNORE_VERSION 0 patch to ignore list...", LogLevel.WARNING);
                List<ulong> _tmp = new List<ulong>(JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText("config/FunModule_RandomGame_Ignores.cfg")));
                foreach (ulong a in _tmp)
                {
                    this.Logger.Log("Adding ignore entry of type {0} for UID {1}", GameIgnoreEntry.IgnoreType.USER, a);
                    this.ignoredEntities.Add(new GameIgnoreEntry(a, GameIgnoreEntry.IgnoreType.USER));
                }
                this.Logger.Log("IGNORE_VERSION 0 patch done.", LogLevel.WARNING);
                this.saveIgnoredUsers();
                return;
            }
            else
            {
                foreach(JObject jo in _json["ignoredEntities"])
                {
                    ulong id = Convert.ToUInt64(jo["UniqueID"].ToString());
                    GameIgnoreEntry.IgnoreType ignoretype = (GameIgnoreEntry.IgnoreType)(Convert.ToInt32(jo["Type"].ToString()));
                    this.Logger.Log("Adding ignore entry of type {0} for UID {1}", ignoretype, id);
                    this.ignoredEntities.Add(new GameIgnoreEntry(id, ignoretype));
                }
            }
            this.Logger.Log("Loaded {0} ignores from config", this.ignoredEntities.Count);
        }

        public void saveIgnoredUsers()
        {
            string json = JsonConvert.SerializeObject(this.ignoredEntities);
            string combined = JsonConvert.SerializeObject(new { IGNORE_VERSION, this.ignoredEntities }, Formatting.Indented);
            File.WriteAllText("config/FunModule_RandomGame_Ignores.cfg", combined);
        }

    }
}
