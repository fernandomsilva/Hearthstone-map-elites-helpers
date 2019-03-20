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

public static class StringExtensions
{
    public static string[] Split2(this string source, char delim)
    {
        // argument null checking etc omitted for brevity
		List<string> result = new List<string>();

        int oldIndex = 0, newIndex;
        while ((newIndex = source.IndexOf(delim, oldIndex)) != -1)
        {
            result.Add(source.Substring(oldIndex, newIndex - oldIndex));
            oldIndex = newIndex + 1;//delim.Length;
        }
		result.Add(source.Substring(oldIndex));
        
		return result.ToArray();
    }
}

namespace GamePlayer
{
    internal class Program
    {
        private static int maxDepth; //= 13;//maxDepth = 10 and maxWidth = 500 is optimal 
        private static int maxWidth; //= 4;//keep maxDepth high(around 13) and maxWidth very low (4) for maximum speed
        //private static int parallelThreads = 1;// number of parallel running threads//not important
        //private static int testsInEachThread = 1;//number of games in each thread//ae ere
                                                 //you are advised not to set more than 3 parallel threads if you are doing this on your laptop, otherwise the laptop will not survive
        //private static int parallelThreadsInner = 1;//this his what is important
        //private static int testsInEachThreadInner = 2;//linearly

		private static int GPUID;
		private static string folderName;
		private static int numGames;
		private static int stepSize;
		private static bool record_log;
		private static string player_decks_file;
		private static string opponent_decks_file;
		
		public static void parseArgs(string[] args)
		{
			for (int i=0; i<args.Length; i++)
			{
				string argument = args[i].ToLower();
				
				if (argument.Contains("gpuid="))
				{
					GPUID = int.Parse(argument.Substring(6)) - 1;
				}
				else if (argument.Contains("folder="))
				{
					folderName = args[i].Substring(7);
				}
				else if (argument.Contains("numgames="))
				{
					numGames = int.Parse(argument.Substring(9));
				}
				else if (argument.Contains("stepsize="))
				{
					stepSize = int.Parse(argument.Substring(9));
				}
				else if (argument.Contains("playerdecks="))
				{
					player_decks_file = argument.Substring(12);
				}
				else if (argument.Contains("opponentdecks="))
				{
					opponent_decks_file = argument.Substring(14);
				}
				else if (argument.Contains("log="))
				{
					if (argument[4] == 't')
					{
						record_log = true;
					}
					else
					{
						record_log = false;
					}
				}
				else if (argument.Contains("nerf="))
				{
					string nerf_data_filepath = argument.Substring(5);
					
					NerfCards(nerf_data_filepath);
				}
				else if (argument.Contains("maxwidth="))
				{
					maxWidth = int.Parse(argument.Substring(9));
				}
				else if (argument.Contains("maxdepth="))
				{
					maxDepth = int.Parse(argument.Substring(9));
				}
			}
		}
		
		private static void NerfCards(string nerf_filepath)
		{
			List<string[]> nerfs = new List<string[]>();

            string[] file_data = System.IO.File.ReadAllLines(nerf_filepath);
			
			for (int i=0; i<file_data.Length; i++)
			{
				string textLine = file_data[i];				
				string[] nerf_data = textLine.Split2(';');
				nerfs.Add(nerf_data);
			}
			
			foreach (string[] nerf in nerfs)
			{
				Card nerfed_card = Cards.FromName(nerf[0]);
				
				if (!string.IsNullOrEmpty(nerf[1]))
				{
					nerfed_card.Tags[GameTag.COST] = int.Parse(nerf[1]);
				}
				if (!string.IsNullOrEmpty(nerf[2]))
				{
					nerfed_card.Tags[GameTag.ATK] = int.Parse(nerf[2]);
				}
				if (!string.IsNullOrEmpty(nerf[3]))
				{
					nerfed_card.Tags[GameTag.HEALTH] = int.Parse(nerf[3]);
				}
			}
		}

        private static void Main(string[] args)
        {
			Stopwatch timer = new Stopwatch();
			
			List<List<object>> players; //= new List<List<object>>();
			List<List<object>> opponents; //= new List<List<object>>();

            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 1;//parallelThreads;

			maxDepth = 13;
			maxWidth = 4;
			GPUID = 0;
			folderName = "";
			numGames = 2;
			stepSize = 0;
			player_decks_file = "player_decks.csv";
			opponent_decks_file = "opponent_decks.csv";
			
			parseArgs(args);
			
			Console.WriteLine("maxDepth = " + maxDepth);
			Console.WriteLine("maxWidth = " + maxWidth);
			
			if (folderName == "") { folderName = DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss"); }
			
			if (stepSize == 0) { stepSize = numGames; }
			if (numGames < stepSize || (numGames % stepSize) > 0)
			{
				Console.WriteLine("\'numGames / stepSize\' must result in an integer bigger than 0");
				return;
			}
			
			int number_of_loops = numGames / stepSize;
			
			players = getPlayersFromFile(player_decks_file);
			opponents = getPlayersFromFile(opponent_decks_file);
			
			if (!Directory.Exists(folderName))
			{
				Directory.CreateDirectory(folderName);
			}
			Thread.Sleep(10000);

			int j = 0;
			
			timer.Start();
			
			foreach (List<object> player in players)
			{
				foreach (List<object> opponent in opponents)
				{
					j = 0;
					
					while (j < number_of_loops)
					{
						string winRate = "";
						bool retry = true;
						int tries = 0;
						
						while (retry)
						{
							try
							{
								string player1Class = (string) player[1];
								string player1Strategy = (string) player[2];
								List<Card> player1Deck = (List<Card>) player[3];
								string player2Class = (string) opponent[1];
								string player2Strategy = (string) opponent[2];
								List<Card> player2Deck = (List<Card>) opponent[3];
								
								//Console.WriteLine("Start Thread");
								var thread = new Thread(() =>
								{
									winRate = getWinRate(player1Class, player1Strategy, player1Deck, player2Class, player2Strategy, player2Deck);
								});
								
								thread.Start();
								
								bool finished = thread.Join(600000);

								//Console.WriteLine("Thread End");
								
								if (!finished)
								{
									retry = true;

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
						
						string overallGameStat = folderName + "/" + player[0] + "/" + opponent[0];
						if (!Directory.Exists(overallGameStat))
						{
							Directory.CreateDirectory(overallGameStat);
						}
						try
						{
							overallGameStat = overallGameStat + "/Output-" + GPUID + "-" + j + ".txt";
							using (StreamWriter tw = File.AppendText(overallGameStat))
							{
								tw.WriteLine(winRate);

								tw.Close();
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
						j++;
					}
				}
			}
			
			timer.Stop();
			Console.WriteLine("Time: {0}", timer.Elapsed);
        }
		
		public static void registerLogPlayStats(string log_text, ref Dictionary<string, int[]> playerCardPlayCount, bool won)
		{
			string visited = "";

			foreach(var line in log_text.Split2('\n'))//new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Contains("[Player 1] play"))
				{
					string key = line.Split2('\'')[1];
					key = key.Split2('[')[0];
					if (playerCardPlayCount.ContainsKey(key))
					{
						if (!visited.Contains(key))
						{
							playerCardPlayCount[key][0] = playerCardPlayCount[key][0] + 1;
							if (won)
							{
								playerCardPlayCount[key][1] = playerCardPlayCount[key][1] + 1;
							}
						}
						else
						{
							playerCardPlayCount[key][2] = playerCardPlayCount[key][2] + 1;
							if (won)
							{
								playerCardPlayCount[key][3] = playerCardPlayCount[key][3] + 1;
							}
						}
					}
					else
					{
						playerCardPlayCount[key] = new int[6];
						playerCardPlayCount[key][0] = 1;
						if (won)
						{
							playerCardPlayCount[key][1] = 1;
						}
						playerCardPlayCount[key][2] = 0;
						playerCardPlayCount[key][3] = 0;
					}
						
					visited += key + " , ";
				}
				else if (line.Contains("Hand<P1>"))
				{
					registerLogHandStats(line, ref playerCardPlayCount, won);
				}
			}
		}
		
		public static void registerLogHandStats(string log_text, ref Dictionary<string, int[]> playerCardPlayCount, bool won)
		{
			string visited = "";
			
			foreach (var name in log_text.Split2(','))
			{
				if (!name.Contains("Hand<P1>") && !string.Equals(name, ""))
				{
					if (!visited.Contains(name))
					{
						if (playerCardPlayCount.ContainsKey(name))
						{
							playerCardPlayCount[name][4] = playerCardPlayCount[name][4] + 1;
							if (won)
							{
								playerCardPlayCount[name][5] = playerCardPlayCount[name][5] + 1;
							}
						}
						else
						{
							playerCardPlayCount[name] = new int[6];
							playerCardPlayCount[name][4] = 1;
							if (won)
							{
								playerCardPlayCount[name][5] = 1;
							}
							playerCardPlayCount[name][2] = 0;
							playerCardPlayCount[name][3] = 0;
						}
						
						visited += name + " , ";
					}
				}
			}
		}

		public static string getWinRate(string player1Class, string player1Strategy, List<Card> player1Deck, string player2Class, string player2Strategy, List<Card> player2Deck)
        {
            int[] wins = Enumerable.Repeat(0, stepSize).ToArray();
			string[] game_log_list = new string[stepSize];

			Dictionary<string, int[]> playerCardPlayCount = new Dictionary<string, int[]>();
			
            ParallelOptions parallel_options = new ParallelOptions();
            parallel_options.MaxDegreeOfParallelism = 8;// parallelThreads;// Environment.ProcessorCount;//parallelThreadsInner+10;
                                                       // Console.WriteLine(Environment.ProcessorCountCount);
													   
			for (int k=0; k<game_log_list.Length; k++)
			{
				game_log_list[k] = "";
			}
			
            string res = "";
            Parallel.For(0, stepSize, parallel_options, j =>//parallelThreadsInner * testsInEachThreadInner, parallel_options, j =>
            {
                int i = j;
				//Console.WriteLine(i);

                string s = "";
				string game_log = "";
                bool retry = true;
                
				while (retry)
                {
                    try
                    {
						//Console.WriteLine("Start Game!");
                        s = FullGame(player1Class, player1Strategy, player1Deck, player2Class, player2Strategy, player2Deck, ref game_log);
						game_log_list[j] = game_log;
						//Console.WriteLine("Game End!");
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

                if (s.Contains("Player1: WON"))
                {
                    wins[i]++;
                }
            });

			res = "" + stepSize + ";" + wins.Sum() + ";\n";
			
			if (record_log)
			{
				for (int k=0; k<game_log_list.Length; k++)
				{
					registerLogPlayStats(game_log_list[k], ref playerCardPlayCount, game_log_list[k].Contains("Player1: WON"));
				}
				foreach (KeyValuePair<string, int[]> kvp in playerCardPlayCount)
				{
					res = res + kvp.Key + ";" + kvp.Value[0] + ";" + kvp.Value[1] + ";" + kvp.Value[2] + ";" + kvp.Value[3] + ";" + kvp.Value[4] + ";" + kvp.Value[5] + ";\n";
				}
			}
			
            return res;
        }

        public static List<List<object>> getPlayersFromFile(string path)
        {
			List<List<object>> result = new List<List<object>>();

            string[] file_data = System.IO.File.ReadAllLines(path);
			
			for (int i=0; i<file_data.Length; i++)
			{
				string textLines = file_data[i];
				List<object> new_entry = new List<object>();
				
				string[] playerInfo = textLines.Split2(';');
				string[] cards = playerInfo[3].Split2('*');
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

        public static string FullGame(string player1Class, string player1Strategy, List<Card> player1Deck, string player2Class, string player2Strategy, List<Card> player2Deck, ref string gameLogAddr)
        {
            string logsbuild = "";
            var game = getGame(player1Class, player1Deck, player2Class, player2Deck);

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
			
			string hand_log = "Hand<P1>,";
			string temp_hand_log = "";

            game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
            game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

            game.MainReady();

            while (game.State != State.COMPLETE)
            {
				logsbuild += $"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " +
                    $"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}" + "\n";
                logsbuild += $"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}" + "\n";
				for (int i=0; i < game.Player1.HandZone.Count; i++)
				{
					temp_hand_log = $"{game.Player1.HandZone[i]}";
					hand_log += temp_hand_log.Substring(1, temp_hand_log.Length-1);
					hand_log = hand_log.Split2('[')[0] + ",";
				}
                logsbuild += "\n";

				//Console.WriteLine(logsbuild);
				//registerLogHandStats(logsbuild);
				
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
            }
			
			//hand_log = hand_log + "\n";
			//Console.WriteLine(hand_log);
			
            int healthdiff = game.Player1.Hero.Health - game.Player2.Hero.Health;
            logsbuild += "Game: {game.State}, Player1: " + game.Player1.PlayState + " / Player2:" + game.Player2.PlayState + "healthdiff:" + healthdiff + "& turns:" + game.Turn;
			gameLogAddr = logsbuild + "\n" + hand_log;

            return "start player=" + startPlayer + ", Game: {game.State}, Player1: " + game.Player1.PlayState + " / Player2:" + game.Player2.PlayState + "healthdiff:" + healthdiff + "& turns:" + game.Turn;
        }
    }
}

