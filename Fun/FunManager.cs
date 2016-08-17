using Discord;
using DiscordBot.Extensions;
using DiscordBot.Logging;
using DiscordBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Fun
{
    public class FunManager
    {

        // These are mostly sattire, but there's some legit games in here too
        private string[] _gameNames = new string[] {
            "Kerbal Space Program",
            "Launch Schedule Simulator 2016",
            "Elite: Dangerous",
            //"010100110111000001100001011000010110000101100001011000110110010100100001", <- not a game
            "0011010000110010", // <- not a game
            "Universe Sandbox 2",
            "Euro Truck Simulator 2",
            "American Truck Simulator",
            "Portal 2",
            "The Waiting Game",
            "The Price Is Right",
            "No Man's Sky",
            "Half-Life 3", // How can you not have a random game easter egg that doesn't have HL3 in it?
            "With Fire",
            "Doctor",
            "Theme Hospital", // Hospital administrator is TOTALLY cheating
            "Bowling with Roman"
        };

        private bool isPlayingGame = false;
        private int newGameMagicNumber = 1;
        private DateTime startedPlaying = new DateTime(1970, 1, 1);
        private double _gamePlayingGraceMinutes = 30; // How many minutes should we "play" a game for minimum, even if the random chance is hit again?

        public static FunManager Instance { get; private set; }
        private Logger Logger { get; set; }

        public FunManager()
        {
            Instance = this;
            this.Logger = new Logger("FunManager");
#if DEBUG
            newGameMagicNumber = 50;
#endif
            //this.Logger.Log("Let's go have fun, Niko!");
        }

        public string onMessageReceived(MessageEventArgs e, bool forced = false)
        {

            Random r = new Random();
            int _newGame = r.RealNext(100);
            //this.Logger.Log("{0}", _newGame);
            if (_newGame <= newGameMagicNumber || forced) // N% chance hit, pick a new game (or not)
            {
                //this.Logger.Log("Random chance on GameChance was hit -- {0}", DateTime.Compare(this.startedPlaying.AddMinutes(_gamePlayingGraceMinutes), DateTime.Now));
                if (this.isPlayingGame && DateTime.Compare(this.startedPlaying.AddMinutes(_gamePlayingGraceMinutes), DateTime.Now) < 1 || forced)
                {
                    int whatDo = (new Random(Utils.getEpochTime())).RealNext(1, 10); // If this is >= 5, we do nothing
                    if (whatDo == 1) { this.Logger.Log("Stopped playing"); Program.Instance.client.SetGame(null); this.isPlayingGame = false; } // 10%, stop "playing"
                    else if (whatDo >= 2 && whatDo < 5) // 40%, pick a new game
                    {
                        return setRandomGame();
                    }
                    // else do nothing
                }
                else if (!this.isPlayingGame)
                {
                    this.isPlayingGame = true;
                    return setRandomGame();
                }

            }
            return string.Empty;
        }

        private string setRandomGame()
        {
            this.startedPlaying = DateTime.Now;
            string game = _gameNames[new Random(Utils.getEpochTime()).Next(_gameNames.Length)];
            this.Logger.Log("Now playing: {0}", game);
            Program.Instance.client.SetGame(new Game(game));
            return game;
        }




    }
}
