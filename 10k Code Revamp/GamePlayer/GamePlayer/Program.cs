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
        static string friendDeckClass = "";
        static string enemyDeckClass = ""; 

        private static void Main(string[] args)
        {
            Dictionary<int, Dictionary<int, List<Card>>> victoryMany = new Dictionary<int, Dictionary<int, List<Card>>>();

            Dictionary<int, float> winRates = new Dictionary<int, float>();

            Dictionary<int, string> results = new Dictionary<int, string>();
            Dictionary<int, List<Card>> resultsMutated = new Dictionary<int, List<Card>>();

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

            Console.WriteLine("here for Hunter Deck GPUID=" + GPUID + "numgames=" + numGames + "folderanme=" + folderName);
            Console.WriteLine("here GPUID=" + GPUID + "numgames=" + numGames + "folderanme=" + folderName);
            string path = folderName + "/Decks.txt";
            int level = int.Parse(folderName.Split('-')[1]);
            while (true)
            {
                while (Directory.Exists(folderName))
                {
                    Console.WriteLine(" this code is for Fernando to run on Sir HPC count of results=" + results.Count);
                    Console.WriteLine("Found " + folderName);
                    Thread.Sleep(10000);
                    path = folderName + "/Decks.txt";

                    int j = 0;
                    int currDeckID = deckID;
                    while (j < 25)
                    {
                        List<Card> playerDeck2 = getDeckFromFile(folderName+"/EnemyDeck.txt");
                        List<Card> playerDeck = getDeckFromFile(folderName+"/FriendDeck.txt"); //nDecks[currDeckID];
                        Console.WriteLine("Enemy deck:-size=" + playerDeck2.Count);
                        //createMutateObj.print(playerDeck2);
                        Console.WriteLine("Friend deck:- size=" + playerDeck.Count);
                        //createMutateObj.print(playerDeck);
                        string gameLogAddr = folderName + "/Deck" + currDeckID;
                        enemyDeckClass = getClassFromFile(folderName + "/EnemyDeck.txt");
                        friendDeckClass = getClassFromFile(folderName + "/FriendDeck.txt");
                        Console.WriteLine("currently on deck =" + currDeckID);
                        gameLogAddr += "/" + GPUID + "-" + j + ".txt";

                        //createMutateObj.printToFile(playerDeck, gameLogAddr);//printed once in begining
                        Stopwatch stopwatch = new Stopwatch();
                        if (!results.ContainsKey(j))
                        {
                            string winRate_timeMean = "";
                            stopwatch.Start();
                            bool retry = true;
                            int tries = 0;
                            while (retry)
                            {
                                try
                                {
                                    var thread = new Thread(() =>
                                    {
                                        winRate_timeMean = "GPUID:" + GPUID + "-" + j + " Game Deck:" + currDeckID + ": " + getWinRateTimeMean(playerDeck, j, playerDeck2, gameLogAddr);
                                    });
                                    thread.Start();
                                    bool finished = thread.Join(600000);
                                    if (!finished)
                                    {
                                        retry = true;
                                        Console.WriteLine("had to continue here for deck=" + currDeckID);

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
                            Console.WriteLine("here after game is played GPUID=" + GPUID + "-" + j);

                            stopwatch.Stop();

                            long seconds = (stopwatch.ElapsedMilliseconds / 1000);//(stop - start).ToString();//
                            TimeSpan t = TimeSpan.FromSeconds(seconds);

                            if (!results.ContainsKey(j))
                            {
                                results.Add(j, winRate_timeMean + "Time taken:" + t.ToString());
                            }

                            string cardStatisticsForPrint = produceCardStatsString();

                            stopwatch.Reset();
                            Console.WriteLine("here just before file print GPUID=" + GPUID + "-" + j + "numgames=" + numGames + "folderanme=" + folderName);
                            string overallGameStat = folderName + "/Overall/Deck" + currDeckID;
                            if (!Directory.Exists(overallGameStat))
                            {
                                Directory.CreateDirectory(overallGameStat);
                            }
                            try
                            {
                                path = overallGameStat + "/" + GPUID + "-" + j + ".txt";

                                //createMutateObj.printToFile(playerDeck, path);
                                using (StreamWriter tw = File.AppendText(path))
                                {

                                    tw.WriteLine(results[j]);
                                    tw.WriteLine(cardStatisticsForPrint + "/n");

                                    //print card stats here.
                                    tw.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            foreach (int key in results.Keys)
                            {
                                if (resultsMutated.ContainsKey(key))
                                {
                                    //  createMutateObj.print(resultsMutated[key]);
                                }
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

                    Console.WriteLine("trying to find " + folderName);
                }
            }
            foreach (int key in results.Keys)
            {
                Console.WriteLine("Game " + key + " : " + results[key] + "\n");
                if (resultsMutated.ContainsKey(key))
                {
                    //createMutateObj.print(resultsMutated[key]);
                }
                stopwatch2.Stop();
                TimeSpan tempeForOverall = TimeSpan.FromSeconds(stopwatch2.ElapsedMilliseconds / 1000);
                Console.WriteLine("Overall time taken:" + tempeForOverall.ToString());
            }
        }

        public static string getWinRateTimeMean(List<Card> player1Deck, int where, List<Card> player2Deck, string gameLogAddr)
        {
            int[] wins = Enumerable.Repeat(0, 1000).ToArray();
            long sum_Timetaken = 0;
            int winss = 0;
            /*object[] temp()
            {
                object[] obj = new object[1002];
                for (int i = 0; i < parallelThreadsInner * testsInEachThreadInner; i++)
                {
                    obj[i] = new Stopwatch();
                }
                return obj;
            }*/
            ParallelOptions parallel_options = new ParallelOptions();
            parallel_options.MaxDegreeOfParallelism = 8;// parallelThreads;// Environment.ProcessorCount;//parallelThreadsInner+10;
                                                       // Console.WriteLine(Environment.ProcessorCount);
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
                        s = FullGame(player1Deck, i, player2Deck, gameLogAddr);
                        Console.WriteLine(s);
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

        public static List<Card> getDeckFromFile(string path)
        {
            List<Card> opponent = new List<Card>();
            string[] textLines = System.IO.File.ReadAllLines(path);
            Console.WriteLine("lines size=" + textLines.Length + "in path=" + path);
            string[] cards = textLines[1].Split('*');
            for (int i = 0; i < 30; i++)
            {
                opponent.Add(Cards.FromName(cards[i]));
            }
            return opponent;
        }

        public static string getClassFromFile(string path)
        {
            List<Card> opponent = new List<Card>();
            string[] textLines = System.IO.File.ReadAllLines(path);
            Console.WriteLine("lines size=" + textLines.Length + "in path=" + path);
            string Class = textLines[0].Trim().ToLower();
            return Class;
        }

        public static Game getGame(List<Card> player1Deck, List<Card> player2Deck)
        {
            var friendClass = CardClass.HUNTER;
            var enemyClass = CardClass.HUNTER;
            if(friendDeckClass.ToLower().StartsWith("paladin"))
            { friendClass = CardClass.PALADIN; Console.WriteLine("friend(player1) is paladin");}
            else if (friendDeckClass.ToLower().StartsWith("hunter"))
            { friendClass = CardClass.HUNTER; Console.WriteLine("friend(player1) is hunter"); }
            else if (friendDeckClass.ToLower().StartsWith("warlock"))
            { friendClass = CardClass.WARLOCK; Console.WriteLine("friend(player1) is warlock"); }
            else if (friendDeckClass.ToLower().StartsWith("shaman"))
            { friendClass = CardClass.SHAMAN; Console.WriteLine("friend(player1) is shaman"); }
            else if (friendDeckClass.ToLower().StartsWith("druid"))
            { friendClass = CardClass.DRUID; Console.WriteLine("friend(player1) is druid"); }
            else if (friendDeckClass.ToLower().StartsWith("mage"))
            { friendClass = CardClass.MAGE; Console.WriteLine("friend(player1) is mage"); }
            else if (friendDeckClass.ToLower().StartsWith("priest"))
            { friendClass = CardClass.PRIEST; Console.WriteLine("friend(player1) is priest"); }
            else if (friendDeckClass.ToLower().StartsWith("rogue"))
            { friendClass = CardClass.ROGUE; Console.WriteLine("friend(player1) is rogue"); }
            else if (friendDeckClass.ToLower().StartsWith("warrior"))
            { friendClass = CardClass.WARRIOR; Console.WriteLine("friend(player1) is warrior"); }

            if (enemyDeckClass.ToLower().StartsWith("paladin"))
            { enemyClass = CardClass.PALADIN; Console.WriteLine("enemy(player2) is paladin");}
            else if (enemyDeckClass.ToLower().StartsWith("hunter"))
            { enemyClass = CardClass.HUNTER; Console.WriteLine("enemy(player2) is hunter"); }
            else if (enemyDeckClass.ToLower().StartsWith("warlock"))
            { enemyClass = CardClass.WARLOCK; Console.WriteLine("enemy(player2) is warlock"); }
            else if (enemyDeckClass.ToLower().StartsWith("shaman"))
            { enemyClass = CardClass.SHAMAN; Console.WriteLine("enemy(player2) is shaman"); }
            else if (enemyDeckClass.ToLower().StartsWith("druid"))
            { enemyClass = CardClass.DRUID; Console.WriteLine("enemy(player2) is druid"); }
            else if (enemyDeckClass.ToLower().StartsWith("mage"))
            { enemyClass = CardClass.MAGE; Console.WriteLine("enemy(player2) is mage"); }
            else if (enemyDeckClass.ToLower().StartsWith("priest"))
            { enemyClass = CardClass.PRIEST; Console.WriteLine("enemy(player2) is priest"); }
            else if (enemyDeckClass.ToLower().StartsWith("rogue"))
            { enemyClass = CardClass.ROGUE; Console.WriteLine("enemy(player2) is rogue"); }
            else if (enemyDeckClass.ToLower().StartsWith("warrior"))
            { enemyClass = CardClass.WARRIOR; Console.WriteLine("enemy(player2) is warrior"); }

            var game = new Game(
            new GameConfig()
            {
                StartPlayer = -1,
                Player1Name = "FitzVonGeraldPaladin",
                Player1HeroClass = friendClass,
                Player1Deck = player1Deck,
                Player2Name = "RehHausZuckFuchsPaladin",
                Player2HeroClass = enemyClass,
                Player2Deck = player2Deck,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = false
            });
            return game;
        }

        public static string FullGame(List<Card> player1Deck, int where, List<Card> player2Deck, string gameLogAddr)
        {
            string logsbuild = "";
            var game = getGame(player1Deck, player2Deck);
            Console.WriteLine("game heroes:" + game.Heroes[0].ToString() + " and " + game.Heroes[1].ToString());
            game.StartGame();
            string startPlayer = game.CurrentPlayer.Name;
            var aiPlayer1 = new AggroScore();
            var aiPlayer2 = new AggroScore();

            List<int> mulligan1 = aiPlayer1.MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            List<int> mulligan2 = aiPlayer2.MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
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

                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, aiPlayer1, maxDepth, maxWidth);
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
                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player2.Id, aiPlayer2, maxDepth, maxWidth);
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

            return "start player=" + startPlayer + ", Game: {game.State}, Player1: " + game.Player1.PlayState + " / Player2:" + game.Player2.PlayState + "healthdiff:" + healthdiff + "& turns:" + game.Turn;
        }
        static Dictionary<int, Dictionary<string, int>> calcuated = new Dictionary<int, Dictionary<string, int>>();//global
        static void calcukateCardFreq(string printed, int idgame, Dictionary<string, int> cardnames)
        {

            string namecard = "";

            if (cardnames.ContainsKey(namecard))
            {
                calcuated[idgame].Add(namecard, 0);
            }
        }
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

