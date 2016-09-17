using Discord;
using DiscordBot.Extensions;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Fun
{
    public class FunModuleRandomGame : FunModuleBase, IFunModule
    {

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

        public FunModuleRandomGame()
        {
            this.Logger = FunManager.Instance.Logger.createSubLogger("RandomGame");
            this.Logger.Log("Chance of playing or changing game is {0:P2} ({1}/{2})", (decimal)newGameMagicNumber / (decimal)newGameRollCeiling, newGameMagicNumber, newGameRollCeiling);
            this.Logger.Log("Attempting to load total gameplay times from config...");
            this.loadGamesTimes();
            this.loadGameList();
            this.loadGameBlacklist();
        }

        public override void onUserUpdate(UserUpdatedEventArgs e)
        {
            if (!e.After.Name.Equals("LaunchBot") && e.After.CurrentGame.HasValue && (e.After.CurrentGame.Value.Url == null || e.After.CurrentGame.Value.Url == string.Empty))
            {
                string gameName = e.After.CurrentGame.Value.Name;
                if (!this._blacklistedGames.Contains(gameName) && !gameName.ContainsIgnoreCase("Minecraft") && !this._gameNames.Contains(gameName))
                {
                    this._gameNames.Add(gameName);
                    this.saveGameList();
                    this.Logger.Log("Learnt a new game: {0}", gameName);
                    Utils.sendToDebugChannel("[**RandomGame**] Learnt a new game from '{1}': {0}", gameName, e.After.Name);
                }
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
            if (this.isPlayingGame && !this.currentGame.Equals(Program.Instance.client.CurrentGame.Name))
            {
                Program.Instance.client.SetGame(this.currentGame);
            }
        }

        public override void onMessageReceived(MessageEventArgs e)
        {

            Random r = new Random();
            int _newGame = r.RealNext(newGameRollCeiling);
            //this.Logger.Log("{0}", _newGame);
            if (_newGame <= newGameMagicNumber) // N% chance hit, pick a new game (or not)
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
        }

        /*public override void onBotTerminating()
        {
            this.saveCurrentGameTime();
            //this.saveGameTimes();
        }*/

        private void setRandomGame()
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

    }
}
