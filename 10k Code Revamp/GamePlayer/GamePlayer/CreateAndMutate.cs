using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabberStoneCore.Model;
using System.IO;
namespace GamePlayer
{
    class CreateAndMutate
    {
        public Dictionary<int, int> convertListtoDict(List<Card> toMutate, Dictionary<string, int> cardname)
        {
            Dictionary<int, int> dictToMutate = new Dictionary<int, int>();
            for (int i = 0; i < 30; i++)
            {

                if (dictToMutate.ContainsKey(cardname[toMutate[i].Name]) && dictToMutate[cardname[toMutate[i].Name]] == 1)
                {
                    dictToMutate[cardname[toMutate[i].Name]] = 2;
                }
                if (!dictToMutate.ContainsKey(cardname[toMutate[i].Name]))
                {
                    dictToMutate.Add(cardname[toMutate[i].Name], 1);
                }
            }

            return dictToMutate;
        }

        public Dictionary<int, List<Card>> crossOver(List<Card> Deck1, List<Card> Deck2, int numChilds)
        {
            Dictionary<int, List<Card>> children = new Dictionary<int, List<Card>>();
            for(int i = 0; i < numChilds; i++)
            {
                //Dictionary<int, child = new List<Card>();
                Random rnd = new Random();
                for (int j = 0; j < 30; j++)
                {
                    int chooseDeck = rnd.Next(0, 2);
                    if(chooseDeck > 1)
                    {
                        Console.WriteLine("Error Detected at chooseDeck");
                    }
                    if(chooseDeck == 0)
                    {
                        int chosenCard = rnd.Next(0, 30);

                    }
                }
                
                
            }
            return children;
        }

        public List<Card> mutate(List<Card> toMutate, Dictionary<int, string> allCards, Dictionary<string, int> cardname)
        {
            Dictionary<int, int> dictToMutate = new Dictionary<int, int>();
            dictToMutate = convertListtoDict(toMutate, cardname);
            Random rand = new Random();
            bool swapSuccess = false;
            while (!swapSuccess)
            {

                int oldCard = rand.Next(0, allCards.Count + 1);
                int newCard = rand.Next(0, allCards.Count + 1);
                if (dictToMutate.ContainsKey(oldCard) && allCards.ContainsKey(newCard) && (oldCard != newCard))
                {
                    if (dictToMutate.ContainsKey(newCard) && dictToMutate[newCard] == 2)
                    {
                        swapSuccess = false;
                    }
                    else if (dictToMutate.ContainsKey(newCard) && dictToMutate[newCard] == 1)
                    {
                        swapSuccess = true;
                        dictToMutate[newCard] = 2;
                    }
                    else if (!dictToMutate.ContainsKey(newCard))
                    {
                        swapSuccess = true;
                        dictToMutate.Add(newCard, 1);
                    }
                    else
                    {
                        swapSuccess = false;
                    }
                    if (swapSuccess)
                    {
                        if (dictToMutate[oldCard] == 2)
                        {
                            dictToMutate[oldCard] = 1;
                        }
                        else if (dictToMutate[oldCard] == 1)
                        {
                            dictToMutate.Remove(oldCard);
                        }
                        Console.WriteLine(" ");
                        Console.WriteLine("Swapped " + allCards[oldCard] + " with " + allCards[newCard]);
                        Console.WriteLine(" ");
                    }
                }
                else
                {
                    swapSuccess = false;
                }

            }
            List<Card> mutated = new List<Card>();
            mutated = convertDictToList(dictToMutate, allCards);
            return mutated;
        }

        public List<Card> convertDictToList(Dictionary<int, int> chosenDeck, Dictionary<int, string> allCards)
        {
            //foreach (int key in chosenDeck.Keys)
            {
                //	Console.WriteLine("Key: {0} Value{1} \n", key, chosenDeck[key].ToString());
            }
            //Console.WriteLine("Count:" + Count);
            List<Card> playerDeck = new List<Card>();
            foreach (int key in chosenDeck.Keys)
            {
                for (int i = 1; i <= chosenDeck[key]; i++)
                {
                    playerDeck.Add(Cards.FromName(allCards[key]));
                    //Console.WriteLine(Cards.FromName(allCards[key]) == null);
                }
            }

            return playerDeck;
        }

        public void print(List<Card> listToPrint)
        {
            for (int i = 0; i < 30; i++)
            {
                Console.Write(listToPrint[i].Name+"*");
            }
            Console.WriteLine();
        }
        
        public void printToFile(List<Card> listToPrint, string path)
        {
            string build = "";
            for (int i = 0; i < 30; i++)
            {
                build += listToPrint[i].Name + "*";
            }
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                    using (StreamWriter tw = File.AppendText(path))
                    {
                        tw.WriteLine(build);
                        tw.Close();
                    }
                }
                else
                {
                    using (StreamWriter tw = File.AppendText(path))
                    {
                        tw.WriteLine(build);
                        tw.Close();
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            
        }

        public List<Card> createRandomDeck(Dictionary<int, string> allcards, Dictionary<string, int> cardname)
        {
            Dictionary<int, int> chosenDeck = new Dictionary<int, int>();//dict which will have randomly created deck
            Random rand = new Random();
            int Count = 0;
            while (Count != 30)
            {
                int oneRand = rand.Next(0, allcards.Count + 1);
                if (allcards.ContainsKey(oneRand)) //&& (Cards.FromName(allcards[oneRand])))
                {
                    if (chosenDeck.ContainsKey(oneRand) && (chosenDeck[oneRand] == 1))
                    {
                        chosenDeck[oneRand] = 2;
                        Count++;
                    }
                    else if (!chosenDeck.ContainsKey(oneRand))
                    {
                        chosenDeck.Add(oneRand, 1);
                        Count++;
                    }
                }
            }
            //List<Card> playerDeck = convertDictToList(chosenDeck, allcards);
            Console.WriteLine("Count:" + Count);
            List<Card> createdDeck = convertDictToList(chosenDeck, allcards);
            return createdDeck;
        }
    }
}
