//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;
using GamePlayer.Meta;
using GamePlayer.Nodes;
using GamePlayer.Score;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
//using System.Data.DataSet;
//For Paladin
namespace GamePlayer
{
    internal class Program
    {
        private static readonly Random Rnd = new Random();
        static Dictionary<string, int> cardname = new Dictionary<string, int>();

        private static int maxDepth = 13;//maxDepth = 10 and maxWidth = 500 is optimal 
        private static int maxWidth = 4;//keep maxDepth high(around 13) and maxWidth very low (4) for maximum speed
        private static Dictionary<string, int> cardStats = new Dictionary<string, int>(); //Store the amount of times each card is played PER GAME
        private static int parallelThreads = 1;// number of parallel running threads//not important
        private static int testsInEachThread = 1;//number of games in each thread//ae ere
                                                 //you are advised not to set more than 3 parallel threads if you are doing this on your laptop, otherwise the laptop will not survive
        private static int parallelThreadsInner = 1;//this his what is important
        private static int testsInEachThreadInner = 1;//linearly

        private static bool parallelOrNot = true;

        private static Stopwatch stopwatch2 = new Stopwatch();
        //static string friendDeckClass = "";
        //static string enemyDeckClass = ""; 

        private static void Main(string[] args)
        {
            Dictionary<int, Dictionary<int, List<Card>>> victoryMany = new Dictionary<int, Dictionary<int, List<Card>>>();

            Dictionary<int, float> winRates = new Dictionary<int, float>();

            Dictionary<int, string> results = new Dictionary<int, string>();
            Dictionary<int, List<Card>> resultsMutated = new Dictionary<int, List<Card>>();
			
			List<List<object>> players; //= new List<List<object>>();
			List<List<object>> opponents; //= new List<List<object>>();

            bool end = false;

            stopwatch2.Start();

            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = parallelThreads;

            int GPUID = int.Parse(args[0]) - 1;//1;//1;// -1 for Ms hoover0;// 0;//

            string folderName = args[1].ToString();//"22-02-2018-23-54-44";// "starter-0";/"starter-0";//
            int numGames = int.Parse(args[2]);//200;// 100;// 100;//100;//
            int deckIncrement = int.Parse(args[3]); //2;// 0;//  0;//

            int deckID = GPUID / numGames;
            int remainder = GPUID % numGames;

            //Console.WriteLine("here for Hunter Deck GPUID=" + GPUID + "numgames=" + numGames + "folderanme=" + folderName);
            //Console.WriteLine("here GPUID=" + GPUID + "numgames=" + numGames + "folderanme=" + folderName);
            string path = folderName + "/Decks.txt";
            int level = int.Parse(folderName.Split('-')[1]);
            /*int GPUID = 1;//1;//1;// -1 for Ms hoover0;// 0;//

            string folderName = "";
            int numGames = 0;
            int deckIncrement = 0;

            int deckID = 1;
            int remainder = 1;

            string path = "/Decks.txt";
            int level = 1;*/
			
			players = getPlayersFromFile("test.txt");
			
			/*for (int i=0; i < players.Count; i++)
			{
				Console.WriteLine(players[i][0] + " / " + players[i][1] + " / " + players[i][2]);
				List<Card> deck = (List<Card>) players[i][3];
				for (int j=0; j<deck.Count; j++)
				{
					Console.Write(deck[j] + ", ");
				}
				Console.Write('\n');
			}*/
			
			//List<object> p1 = players[0];
			//List<object> p2 = players[1];
			//string log_text = "";
			
			//Console.WriteLine(FullGame((string) p1[1], (string) p1[2], (List<Card>) p1[3], 0, (string) p2[1], (string) p2[2], (List<Card>) p2[3], ref log_text));
			Dictionary<string, int> playerCardPlayCount = new Dictionary<string, int>();
			//Dictionary<string, int> opponentCardPlayCount = new Dictionary<string, int>();

			foreach(var line in log_text.Split('\n'))//new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Contains("[Player 1] play"))
				{
					string key = line.Split('\'')[1];
					key = key.Split('[')[0];
					if (playerCardPlayCount.ContainsKey(key))
					{
						playerCardPlayCount[key] = playerCardPlayCount[key] + 1;
					}
					else
					{
						playerCardPlayCount[key] = 1;
					}
				}
			}
			
			//foreach (KeyValuePair<string, int> kvp in playerCardPlayCount)
			//{
			//	Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
			//}
			//return;
			
            while (true)
            {
                while (Directory.Exists(folderName))
                {
                    //Console.WriteLine(" this code is for Fernando to run on Sir HPC count of results=" + results.Count);
                    //Console.WriteLine("Found " + folderName);
                    Thread.Sleep(10000);
                    path = folderName + "/Decks.txt";

                    int j = 0;
                    int currDeckID = deckID;
                    while (j < 25)
                    {
                        List<Card> playerDeck2 = getDeckFromFile(folderName+"/EnemyDeck.txt");
                        List<Card> playerDeck = getDeckFromFile(folderName+"/FriendDeck.txt"); //nDecks[currDeckID];
                        //Console.WriteLine("Enemy deck:-size=" + playerDeck2.Count);
                        //Console.WriteLine("Friend deck:- size=" + playerDeck.Count);
                        string gameLogAddr = folderName + "/Deck" + currDeckID;
                        //enemyDeckClass = getClassFromFile(folderName + "/EnemyDeck.txt");
                        //friendDeckClass = getClassFromFile(folderName + "/FriendDeck.txt");
                        //Console.WriteLine("currently on deck =" + currDeckID);
                        gameLogAddr += "/" + GPUID + "-" + j + ".txt";

                        //Stopwatch stopwatch = new Stopwatch();
                        if (!results.ContainsKey(j))
                        {
                            string winRate = "";
                            stopwatch.Start();
                            bool retry = true;
                            int tries = 0;
                            while (retry)
                            {
                                try
                                {
                                    var thread = new Thread(() =>
                                    {
                                        //winRate_timeMean = "GPUID:" + GPUID + "-" + j + " Game Deck:" + currDeckID + ": " + getWinRateTimeMean(playerDeck, j, playerDeck2, gameLogAddr);
										winRate = getWinRate(player1Class, player1Strategy, player1Deck, int where, player2Class, player2Strategy, player2Deck, ref string gameLogAddr);
                                    });
                                    thread.Start();
                                    bool finished = thread.Join(600000);
                                    if (!finished)
                                    {
                                        retry = true;
                                        //Console.WriteLine("had to continue here for deck=" + currDeckID);

                                        tries++;
                                        continue;
                                    }
                                    else
                                    {
                                        retry = false;

                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                                if (tries > 3)
                                {
                                    break;
                                }
                            }

                            if (!results.ContainsKey(j))
                            {
                                results.Add(j, winRate);
                            }
                            j++;
                            currDeckID += deckIncrement;
                            GC.Collect();
                            cardStats.Clear();
                            long memory = GC.GetTotalMemory(false);
                            Console.WriteLine("Memory usage here:" + memory);
                        }
                    }
                    level++;
                    folderName = folderName.Split('-')[0] + "-" + level.ToString();
                    results.Clear();

                    //Console.WriteLine("trying to find " + folderName);
                }
            }
        }

        //public static string getWinRateTimeMean(List<Card> player1Deck, int where, List<Card> player2Deck, string gameLogAddr)
		public static string getWinRate(string player1Class, string player1Strategy, List<Card> player1Deck, int where, string player2Class, string player2Strategy, List<Card> player2Deck, ref string gameLogAddr)
        {
            int[] wins = Enumerable.Repeat(0, 1000).ToArray();
            long sum_Timetaken = 0;
            int winss = 0;

            ParallelOptions parallel_options = new ParallelOptions();
            parallel_options.MaxDegreeOfParallelism = 8;// parallelThreads;// Environment.ProcessorCount;//parallelThreadsInner+10;
                                                       // Console.WriteLine(Environment.ProcessorCountCount);
            //object[] stopwatches = temp();
            string res = "";
            Parallel.For(0, parallelThreadsInner * testsInEachThreadInner, parallel_options, j =>
            {
                int i = j;

                //((Stopwatch)stopwatches[i]).Start();

                string s = "";
                bool retry = true;
                while (retry)
                {
                    try
                    {
                        s = FullGame(player1Class, player1Deck, i, player2Class, player2Deck, ref gameLogAddr);
                        //Console.WriteLine(s);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        s = e.Message.ToString();
                    }

                    if (s.ToLower().Contains("present") || s.ToLower().Contains("instance") || s.ToLower().Contains("zone"))
                    {
                        Console.WriteLine("this was s=" + s + "retrying right here");

                        retry = true;
                    }
                    else
                    {
                        retry = false;
                    }
                }
                //((Stopwatch)stopwatches[i]).Stop();
                //long seconds = (((Stopwatch)stopwatches[i]).ElapsedMilliseconds / 1000);

                //sum_Timetaken = sum_Timetaken + seconds;

                if (s.Contains("Player1: WON"))
                {
                    wins[i]++;
                }

                res = s;
            });
            //TimeSpan t = TimeSpan.FromSeconds(sum_Timetaken / (parallelThreadsInner * testsInEachThreadInner));

            return res;// + " and average time of game (hh:mm:ss) = " + t.ToString();
        }

        public static List<List<object>> getPlayersFromFile(string path)
        {
			List<List<object>> result = new List<List<object>>();
            //List<Card> deck = new List<Card>();
            string[] file_data = System.IO.File.ReadAllLines(path);
			
			for (int i=0; i<file_data.Length; i++)
			{
				string textLines = file_data[i];
				List<object> new_entry = new List<object>();
				
				string[] playerInfo = textLines.Split(';');
				string[] cards = playerInfo[3].Split('*');
				List<Card> deck = new List<Card>();
				for (int j = 0; j < 30; j++)
				{
					deck.Add(Cards.FromName(cards[j]));
				}
				
				new_entry.Add(playerInfo[0].Trim());
				new_entry.Add(playerInfo[1].Trim());
				new_entry.Add(playerInfo[2].Trim());
				new_entry.Add(deck);
				
				result.Add(new_entry);
			}
			
            return result;
        }

        public static string getClassFromFile(string path)
        {
            List<Card> opponent = new List<Card>();
            string[] textLines = System.IO.File.ReadAllLines(path);
            Console.WriteLine("lines size=" + textLines.Length + "in path=" + path);
            string Class = textLines[0].Trim().ToLower();
            return Class;
        }

        public static Game getGame(string player1Class, List<Card> player1Deck, string player2Class, List<Card> player2Deck)
        {
			string friendDeckClass = player1Class;
			string enemyDeckClass = player2Class;

            var friendClass = CardClass.HUNTER;
            var enemyClass = CardClass.HUNTER;
						
            if(friendDeckClass.ToLower().StartsWith("paladin"))
            { friendClass = CardClass.PALADIN;}
            else if (friendDeckClass.ToLower().StartsWith("hunter"))
            { friendClass = CardClass.HUNTER; }
            else if (friendDeckClass.ToLower().StartsWith("warlock"))
            { friendClass = CardClass.WARLOCK; }
            else if (friendDeckClass.ToLower().StartsWith("shaman"))
            { friendClass = CardClass.SHAMAN; }
            else if (friendDeckClass.ToLower().StartsWith("druid"))
            { friendClass = CardClass.DRUID; }
            else if (friendDeckClass.ToLower().StartsWith("mage"))
            { friendClass = CardClass.MAGE; }
            else if (friendDeckClass.ToLower().StartsWith("priest"))
            { friendClass = CardClass.PRIEST; }
            else if (friendDeckClass.ToLower().StartsWith("rogue"))
            { friendClass = CardClass.ROGUE; }
            else if (friendDeckClass.ToLower().StartsWith("warrior"))
            { friendClass = CardClass.WARRIOR; }

            if (enemyDeckClass.ToLower().StartsWith("paladin"))
            { enemyClass = CardClass.PALADIN; }
            else if (enemyDeckClass.ToLower().StartsWith("hunter"))
            { enemyClass = CardClass.HUNTER; }
            else if (enemyDeckClass.ToLower().StartsWith("warlock"))
            { enemyClass = CardClass.WARLOCK; }
            else if (enemyDeckClass.ToLower().StartsWith("shaman"))
            { enemyClass = CardClass.SHAMAN; }
            else if (enemyDeckClass.ToLower().StartsWith("druid"))
            { enemyClass = CardClass.DRUID; }
            else if (enemyDeckClass.ToLower().StartsWith("mage"))
            { enemyClass = CardClass.MAGE; }
            else if (enemyDeckClass.ToLower().StartsWith("priest"))
            { enemyClass = CardClass.PRIEST; }
            else if (enemyDeckClass.ToLower().StartsWith("rogue"))
            { enemyClass = CardClass.ROGUE; }
            else if (enemyDeckClass.ToLower().StartsWith("warrior"))
            { enemyClass = CardClass.WARRIOR; }

            var game = new Game(
            new GameConfig()
            {
                StartPlayer = -1,
                Player1Name = "Player 1",
                Player1HeroClass = friendClass,
                Player1Deck = player1Deck,
                Player2Name = "Player 2",
                Player2HeroClass = enemyClass,
                Player2Deck = player2Deck,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = false
            });
            return game;
        }

        public static string FullGame(string player1Class, string player1Strategy, List<Card> player1Deck, int where, string player2Class, string player2Strategy, List<Card> player2Deck, ref string gameLogAddr)
        {
            string logsbuild = "";
            var game = getGame(player1Class, player1Deck, player2Class, player2Deck);
            //Console.WriteLine("game heroes:" + game.Heroes[0].ToString() + " and " + game.Heroes[1].ToString());
            game.StartGame();
            string startPlayer = game.CurrentPlayer.Name;
            object aiPlayer1 = new AggroScore();
            object aiPlayer2 = new AggroScore();

			switch (player1Strategy.ToLower())
			{
				case "control":
					aiPlayer1 = new ControlScore();
					break;
				case "fatigue":
					aiPlayer1 = new FatigueScore();
					break;
				case "midrange":
					aiPlayer1 = new MidRangeScore();
					break;
				case "ramp":
					aiPlayer1 = new RampScore();
					break;
			}
			switch (player2Strategy.ToLower())
			{
				case "control":
					aiPlayer2 = new ControlScore();
					break;
				case "fatigue":
					aiPlayer2 = new FatigueScore();
					break;
				case "midrange":
					aiPlayer2 = new MidRangeScore();
					break;
				case "ramp":
					aiPlayer2 = new RampScore();
					break;
			}

            List<int> mulligan1 = ((GamePlayer.Score.Score) aiPlayer1).MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            List<int> mulligan2 = ((GamePlayer.Score.Score) aiPlayer2).MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            logsbuild += $"Player1: Mulligan {string.Join(",", mulligan1)}";
            logsbuild += "\n";
            logsbuild += $"Player2: Mulligan {string.Join(",", mulligan2)}";
            logsbuild += "\n";

            game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
            game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

            game.MainReady();

            while (game.State != State.COMPLETE)
            {
				logsbuild += $"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " +
                    $"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}" + "\n";
                logsbuild += $"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}" + "\n";
                logsbuild += "\n";

                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player1)
                {
                    logsbuild += $"* Calculating solutions *** Player 1 ***" + "\n";

                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, ((GamePlayer.Score.Score) aiPlayer1), maxDepth, maxWidth);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);

                    logsbuild += $"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------" + "\n";
                    foreach (PlayerTask task in solution)
                    {
                        logsbuild += task.FullPrint() + "\n";

                        string printedTask = task.FullPrint();//CONNOR CODE
                        if (printedTask.IndexOf("play") != -1)//CONNOR CODE
                        {//CONNOR CODE
                            string card = task.Source.ToString();//CONNOR CODE
                            CalculateFreqs(card);//CONNOR CODE
                        }//CONNOR CODE
                        else//CONNOR CODE
                        {//CONNOR CODE
                         // Console.WriteLine("Else: " + printedTask);//CONNOR CODE
                        }//CONNOR CODE
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            logsbuild += $"* Recaclulating due to a final solution ..." + "\n";
                            break;
                        }
                    }
                }
				
                logsbuild += $"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------" + "\n";
                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player2)
                {
                    logsbuild += $"* Calculating solutions *** Player 2 ***" + "\n";
                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player2.Id, ((GamePlayer.Score.Score) aiPlayer2), maxDepth, maxWidth);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);

                    logsbuild += $"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------" + "\n";
                    foreach (PlayerTask task in solution)
                    {
                        logsbuild += task.FullPrint() + "\n";
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            logsbuild += $"* Recaclulating due to a final solution ..." + "\n";
                            break;
                        }
                    }
                }
                GC.Collect();
            }
            int healthdiff = game.Player1.Hero.Health - game.Player2.Hero.Health;
            logsbuild += "Game: {game.State}, Player1: " + game.Player1.PlayState + " / Player2:" + game.Player2.PlayState + "healthdiff:" + healthdiff + "& turns:" + game.Turn;
			gameLogAddr = logsbuild;

            return "start player=" + startPlayer + ", Game: {game.State}, Player1: " + game.Player1.PlayState + " / Player2:" + game.Player2.PlayState + "healthdiff:" + healthdiff + "& turns:" + game.Turn;
        }
        /*static Dictionary<int, Dictionary<string, int>> calcuated = new Dictionary<int, Dictionary<string, int>>();//global
        static void calcukateCardFreq(string printed, int idgame, Dictionary<string, int> cardnames)
        {

            string namecard = "";

            if (cardnames.ContainsKey(namecard))
            {
                calcuated[idgame].Add(namecard, 0);
            }
        }*/
        public static void CalculateFreqs(string thisCard)
        {
            String[] cardDetails = thisCard.Split('[');
            string cardNeeded = cardDetails[0].Remove(0, 1);
            cardNeeded = "[" + cardNeeded + "]";
            if (cardStats.ContainsKey(cardNeeded))
            {
                cardStats[cardNeeded] += 1;
            }
            else
            {
                cardStats.Add(cardNeeded, 1);
            }
        }

        public static string produceCardStatsString()
        {
            string build = "";
            var ordered = cardStats.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (string key in cardStats.Keys)
            {
                build += key + "*" + ordered[key] + "**";
            }

            return build;
        }

    }
}

