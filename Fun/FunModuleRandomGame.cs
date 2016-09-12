﻿using Discord;
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
        public string[] _gameNames = new string[] {
            "Kerbal Space Program",
            "Launch Schedule Simulator 2016",
            "Elite Dangerous",
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
        private int newGameMagicNumber = 150;
#else
        private int newGameMagicNumber = 1;
#endif
        private int newGameRollCeiling = 200; // 1% seemed too common, so I implemented this to enable "lower" percentages
        public DateTime startedPlaying = new DateTime(1970, 1, 1);
        private double _gamePlayingGraceMinutes = 30; // How many minutes should we "play" a game for minimum, even if the random chance is hit again?
        private Logger Logger;
        public string currentGame { get; private set; }
        public Dictionary<string, long> _playTimes = new Dictionary<string, long>();

        public FunModuleRandomGame()
        {
            this.Logger = FunManager.Instance.Logger.createSubLogger("RandomGame");
            this.Logger.Log("Chance of playing or changing game is {0:P2} ({1}/{2})", (decimal)newGameMagicNumber / (decimal)newGameRollCeiling, newGameMagicNumber, newGameRollCeiling);
            this.Logger.Log("Attempting to load total gameplay times from config...");
            this.loadGamesTimes();
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
                        this.Logger.Log("Stopped playing");
                        Program.Instance.client.SetGame(null);
                        this.isPlayingGame = false;
                        this.saveCurrentGameTime();
                    }
                    else if (whatDo >= 2 && whatDo < 5) // 40%, pick a new game
                    {
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

        public override void onBotTerminating()
        {
            this.saveCurrentGameTime();
            this.saveGameTimes();
        }

        private void setRandomGame()
        {
            this.startedPlaying = DateTime.Now;
            string game = _gameNames[new Random(Utils.getEpochTime()).Next(_gameNames.Length)];
            this.currentGame = game;
            this.Logger.Log("Now playing: {0}", game);
            Program.Instance.client.SetGame(new Game(game));
        }

        public void saveCurrentGameTime()
        {
            if (!this.isPlayingGame) { return; }
            int secondsPlayed = ((TimeSpan)(DateTime.Now - this.startedPlaying)).Seconds;
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
            File.WriteAllText("config/FunModule_RandomGame_Times.cfg", JsonConvert.SerializeObject(this._playTimes, Formatting.Indented));
        }

    }
}
