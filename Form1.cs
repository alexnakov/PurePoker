using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Pure_Poker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class Deck
        {
            public static List<string> GetCards()
            /// Returns a shuffled list of all the cards to be used in the deciding a winner logic
            {
                List<string> cards = new List<string>();
                List<string> suits = new List<string>() { "spade", "diamond", "heart", "club" };
                foreach (string suit in suits)
                {
                    for (int i = 2; i <= 9; i++)
                    {
                        cards.Add($"0{i}_{suit}");
                    }
                    for (int i = 10; i <= 14; i++)
                    {
                        cards.Add($"{i}_{suit}");
                    }
                }

                ShuffleList(cards);
                return cards;
            }
            public static List<string> GetPaths(List<string> cards)
            /// Returns the paths to the card images in the same order as the suffled cards
            /// Must be called straight after the GetCards method
            {
                List<string> paths = new List<string>();
                foreach (string card in cards)
                {
                    paths.Add($"F:\\Visual Studio 2019\\cards\\{card}.png");
                }
                return paths;
            }
            static void ShuffleList<T>(IList<T> list)
            /// Shuffles the elements of the list passed
            {
                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                int listLength = list.Count;
                while (listLength > 1)
                {
                    byte[] box = new byte[1];
                    do provider.GetBytes(box);
                    while (!(box[0] < listLength * (Byte.MaxValue / listLength)));
                    int k = (box[0] % listLength);
                    listLength--;
                    T value = list[k];
                    list[k] = list[listLength];
                    list[listLength] = value;
                }
            }
            
            public static List<string> paths;
            public static List<string> cards;
        }
        class Round
        /// Used to track the properties of individual rounds 
        {
            public static int IdentifyTheBigBlind(int number)
            /// Allows us to rotate and keep track of who is the big blind.
            {
                if (number > 2)
                {
                    return number - 2;
                }
                else
                {
                    return number;
                }
            }

            public static int pot = 0;
            public static int bigBlindPosition = 0;
            public static int communityProgression;
        }
        class Player1
        {
            public static List<string> hand = new List<string>();
            public static List<int> handStrength = new List<int>();
            public static int tablePosition = 1;
            public static int chips = 500;
            public static int raiseAmount = 0;
            public static bool check = false;
            public static bool isWinner = false;
        }
        class Player2
        {
            public static List<string> hand = new List<string>();
            public static List<int> handStrength = new List<int>();
            public static int tablePosition = 2;
            public static int chips = 500;
            public static int raiseAmount = 0;
            public static bool check = false;
            public static bool isWinner = false;
        }
        class HandDecider
        {
            public static void Decide()
            {
                EvaluateHand(Player1.hand, Player1.handStrength);
                EvaluateHand(Player2.hand, Player2.handStrength);

                for (int i = 0; i <= Player1.handStrength.Count - 1; i++)
                {
                    if (Player1.handStrength[i] > Player2.handStrength[i])
                    {
                        Player1.isWinner = true;
                        break;
                    }
                    else if (Player2.handStrength[i] > Player1.handStrength[2])
                    {
                        Player2.isWinner = true;
                        break;
                    }
                }

                Player1.hand.Clear(); Player1.handStrength.Clear();
                Player2.hand.Clear(); Player2.handStrength.Clear();
            }
            private static void EvaluateHand(List<string> playerHand, List<int> comparisonHand)
            {
                CheckStraightFlush(playerHand, comparisonHand);
            }
            private static void CheckStraightFlush(List<string> playerHand, List<int> comparisonHand)
            {
                List<string> diamonds = new List<string>();
                List<string> clubs = new List<string>();
                List<string> hearts = new List<string>();
                List<string> spades = new List<string>();

                foreach (string card in playerHand)
                {
                    switch (card.Substring(3, 1))
                    {
                        case "s":
                            spades.Add(card);
                            break;
                        case "h":
                            hearts.Add(card);
                            break;
                        case "d":
                            diamonds.Add(card);
                            break;
                        case "c":
                            clubs.Add(card);
                            break;
                    }
                }

                if (diamonds.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(diamonds));
                    List<int> distinctValues = values.Distinct().ToList();

                    if (distinctValues.Contains(14)) { distinctValues.Add(1); }

                    distinctValues.Sort(); distinctValues.Reverse();

                    for (int i = 0; i <= distinctValues.Count - 5; i++)
                    {
                        if (distinctValues[i] == distinctValues[i + 4] + 4)
                        {
                            if (i == 14) { comparisonHand.Add(9); }
                            else { comparisonHand.Add(8); comparisonHand.Add(distinctValues[i]); }

                            return;
                        }
                    }

                    Check4Kinds(playerHand, comparisonHand);
                }
                else if (hearts.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(hearts));
                    List<int> distinctValues = values.Distinct().ToList();

                    if (distinctValues.Contains(14)) { distinctValues.Add(1); }

                    distinctValues.Sort(); distinctValues.Reverse();

                    for (int i = 0; i <= distinctValues.Count - 5; i++)
                    {
                        if (distinctValues[i] == distinctValues[i + 4] + 4)
                        {
                            if (i == 14) { comparisonHand.Add(9); }
                            else { comparisonHand.Add(8); comparisonHand.Add(distinctValues[i]); }

                            return;
                        }
                    }

                    Check4Kinds(playerHand, comparisonHand);
                }
                else if (spades.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(spades));
                    List<int> distinctValues = values.Distinct().ToList();

                    if (distinctValues.Contains(14)) { distinctValues.Add(1); }

                    distinctValues.Sort(); distinctValues.Reverse();

                    for (int i = 0; i <= distinctValues.Count - 5; i++)
                    {
                        if (distinctValues[i] == distinctValues[i + 4] + 4)
                        {
                            if (i == 14) { comparisonHand.Add(9); }
                            else { comparisonHand.Add(8); comparisonHand.Add(distinctValues[i]); }

                            return;
                        }
                    }

                    Check4Kinds(playerHand, comparisonHand);
                }
                else if (clubs.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(clubs));
                    List<int> distinctValues = values.Distinct().ToList();

                    if (distinctValues.Contains(14)) { distinctValues.Add(1); }

                    distinctValues.Sort(); distinctValues.Reverse();

                    for (int i = 0; i <= distinctValues.Count - 5; i++)
                    {
                        if (distinctValues[i] == distinctValues[i + 4] + 4)
                        {
                            if (i == 14) { comparisonHand.Add(9); }
                            else { comparisonHand.Add(8); comparisonHand.Add(distinctValues[i]); }

                            return;
                        }
                    }

                    Check4Kinds(playerHand, comparisonHand);
                }
                else { Check4Kinds(playerHand, comparisonHand); }

            }
            private static void Check4Kinds(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> playerValues = new List<int>(GetValues(playerHand));
                List<int> values = new List<int>(playerValues);

                IEnumerable<IGrouping<int, int>> multiples = values.GroupBy(v => v);

                foreach (IGrouping<int, int> multiple in multiples)
                {
                    if (multiple.Count() == 4)
                    {
                        comparisonHand.Add(7);
                        comparisonHand.Add(multiple.Key);
                        values.RemoveAll(item => item == multiple.Key);
                        values.Sort(); values.Reverse();
                        comparisonHand.Add(values[0]);
                        return;
                    }
                }

                CheckFullHouse(playerHand, comparisonHand);
            }
            private static void CheckFullHouse(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> playerValues = new List<int>(GetValues(playerHand));
                List<int> values = new List<int>(playerValues);

                values.Sort();
                values.Reverse();

                IEnumerable<IGrouping<int, int>> multiples = values.GroupBy(v => v);

                foreach (IGrouping<int, int> multiple in multiples)
                {
                    if (multiple.Count() == 3)
                    {
                        int triplet = multiple.Key;

                        values.RemoveAll(item => item == triplet);
                        values.Sort(); values.Reverse();

                        IEnumerable<IGrouping<int, int>> pairs = values.GroupBy(v => v);

                        foreach (IGrouping<int, int> pair in pairs)
                        {
                            if (pair.Count() >= 2)
                            {
                                comparisonHand.Add(6);
                                comparisonHand.Add(triplet);
                                comparisonHand.Add(pair.Key);
                                return;
                            }
                        }
                    }
                }

                CheckFlush(playerHand, comparisonHand);
            }
            private static void CheckFlush(List<string> playerHand, List<int> comparisonHand)
            {
                List<string> hand = new List<string>(playerHand);

                List<string> nDiamonds = new List<string>();
                List<string> nClubs = new List<string>();
                List<string> nHearts = new List<string>();
                List<string> nSpades = new List<string>();

                foreach (string card in hand)
                {
                    switch (card.Substring(3, 1))
                    {
                        case "s":
                            nSpades.Add(card);
                            break;
                        case "h":
                            nHearts.Add(card);
                            break;
                        case "d":
                            nDiamonds.Add(card);
                            break;
                        case "c":
                            nClubs.Add(card);
                            break;
                    }
                }

                if (nDiamonds.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(nDiamonds));
                    values.Sort(); values.Reverse();
                    comparisonHand.Add(5);
                    comparisonHand.Add(values[0]);
                    comparisonHand.Add(values[1]);
                    comparisonHand.Add(values[2]);
                    comparisonHand.Add(values[3]);
                    comparisonHand.Add(values[4]);
                }
                else if (nSpades.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(nSpades));
                    values.Sort(); values.Reverse();
                    comparisonHand.Add(5);
                    comparisonHand.Add(values[0]);
                    comparisonHand.Add(values[1]);
                    comparisonHand.Add(values[2]);
                    comparisonHand.Add(values[3]);
                    comparisonHand.Add(values[4]);
                }
                else if (nClubs.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(nClubs));
                    values.Sort(); values.Reverse();
                    comparisonHand.Add(5);
                    comparisonHand.Add(values[0]);
                    comparisonHand.Add(values[1]);
                    comparisonHand.Add(values[2]);
                    comparisonHand.Add(values[3]);
                    comparisonHand.Add(values[4]);
                }
                else if (nHearts.Count > 4)
                {
                    List<int> values = new List<int>(GetValues(nHearts));
                    values.Sort(); values.Reverse();
                    comparisonHand.Add(5);
                    comparisonHand.Add(values[0]);
                    comparisonHand.Add(values[1]);
                    comparisonHand.Add(values[2]);
                    comparisonHand.Add(values[3]);
                    comparisonHand.Add(values[4]);
                }
                else
                {
                    CheckStraight(playerHand, comparisonHand);
                }
            }
            private static void CheckStraight(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> values = new List<int>(GetValues(playerHand));
                List<int> distinctValues = values.Distinct().ToList();

                if (distinctValues.Contains(14)) { distinctValues.Add(1); }

                distinctValues.Sort(); distinctValues.Reverse();

                for (int i = 0; i <= distinctValues.Count - 5; i++)
                {
                    if (distinctValues[i] == distinctValues[i + 4] + 4)
                    {
                        comparisonHand.Add(4);
                        comparisonHand.Add(distinctValues[i]);
                        return;
                    }
                }

                Check3Kinds(playerHand, comparisonHand);
            }
            private static void Check3Kinds(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> playerValues = new List<int>(GetValues(playerHand));
                List<int> values = new List<int>(playerValues);

                values.Sort();
                values.Reverse();

                IEnumerable<IGrouping<int, int>> multiples = values.GroupBy(v => v);

                foreach (IGrouping<int, int> multiple in multiples)
                {
                    if (multiple.Count() == 3)
                    {
                        comparisonHand.Add(3);
                        comparisonHand.Add(multiple.Key);
                        values.RemoveAll(item => item == multiple.Key);
                        comparisonHand.Add(values[0]);
                        comparisonHand.Add(values[1]);
                        return;
                    }
                }

                Check2Pairs(playerHand, comparisonHand);
            }
            private static void Check2Pairs(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> playerValues = new List<int>(GetValues(playerHand));
                List<int> values = new List<int>(playerValues);

                values.Sort();
                values.Reverse();

                IEnumerable<IGrouping<int, int>> multiples = values.GroupBy(v => v);

                foreach (IGrouping<int, int> multiple in multiples)
                {
                    if (multiple.Count() == 2)
                    {
                        int firstPair = multiple.Key;

                        foreach (IGrouping<int, int> pair in multiples)
                        {
                            if (pair.Key != firstPair)
                            {
                                if (pair.Count() == 2)
                                {
                                    comparisonHand.Add(2);
                                    comparisonHand.Add(firstPair);
                                    comparisonHand.Add(pair.Key);
                                    values.RemoveAll(item => item == firstPair);
                                    values.RemoveAll(item => item == pair.Key);
                                    comparisonHand.Add(values[0]);
                                    return;
                                }
                            }
                        }
                    }
                }

                CheckPair(playerHand, comparisonHand);
            }
            private static void CheckPair(List<string> playerHand, List<int> comparisonHand)
            {
                List<int> playerValues = new List<int>(GetValues(playerHand));
                List<int> values = new List<int>(playerValues);

                values.Sort();
                values.Reverse();

                IEnumerable<IGrouping<int, int>> multiples = values.GroupBy(v => v);

                foreach (IGrouping<int, int> multiple in multiples)
                {
                    if (multiple.Count() == 2)
                    {
                        comparisonHand.Add(1);
                        comparisonHand.Add(multiple.Key);
                        values.RemoveAll(item => item == multiple.Key);
                        comparisonHand.Add(values[0]);
                        comparisonHand.Add(values[1]);
                        comparisonHand.Add(values[2]);
                        comparisonHand.Add(values[3]);
                        return;
                    }
                }

                comparisonHand.Add(0);
                comparisonHand.Add(values[0]);
                comparisonHand.Add(values[1]);
                comparisonHand.Add(values[2]);
                comparisonHand.Add(values[3]);
                comparisonHand.Add(values[4]);
            }
            private static List<int> GetValues(List<string> handOfStringCards)
            {
                List<int> values = new List<int>();

                foreach (string card in handOfStringCards)
                {
                    values.Add(Convert.ToInt16(card.Substring(0, 2)));
                }

                return values;
            }
        }

        public void StartNewRound()
        {
            Round.bigBlindPosition++;

            string cardBack = "F:\\Visual Studio 2019\\cards\\back.png";

            cCard1PicBox.Image = Image.FromFile(cardBack);
            cCard2PicBox.Image = Image.FromFile(cardBack);
            cCard3PicBox.Image = Image.FromFile(cardBack);
            cCard4PicBox.Image = Image.FromFile(cardBack);
            cCard5PicBox.Image = Image.FromFile(cardBack);

            Deck.cards = Deck.GetCards();
            Deck.paths = Deck.GetPaths(Deck.cards);

            Player1.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player1.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);

            player1Card1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            player1Card2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            player2Card1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            player2Card2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);

            // Community Cards are addded to both players (logically)
            Player1.hand.Add(Deck.cards[0]); Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player1.hand.Add(Deck.cards[0]); Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player1.hand.Add(Deck.cards[0]); Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player1.hand.Add(Deck.cards[0]); Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);
            Player1.hand.Add(Deck.cards[0]); Player2.hand.Add(Deck.cards[0]); Deck.cards.RemoveAt(0);

            player1CallButton.Enabled = true; player1CheckButton.Enabled = true;
            player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;

            player2CallButton.Enabled = true; player2CheckButton.Enabled = true;
            player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;

            int bigBlind = 10;
            int smallBlind = 5;

            Round.bigBlindPosition = Round.IdentifyTheBigBlind(Round.bigBlindPosition);

            if (Round.bigBlindPosition == 1)
            {
                Player1.chips -= bigBlind;
                Player1.raiseAmount = bigBlind;
                Player2.chips -= smallBlind;
                Player2.raiseAmount = smallBlind;
                
                player1CheckButton.Enabled = false; player1CallButton.Enabled = false;
                player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;
                
                player2CheckButton.Enabled = false;
            }
            else
            {
                Player2.chips -= bigBlind;
                Player2.raiseAmount = bigBlind;
                Player1.chips -= smallBlind;
                Player1.raiseAmount = smallBlind;
                
                player2CheckButton.Enabled = false; player2CallButton.Enabled = false;
                player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                player1CheckButton.Enabled = false;
            }

            Round.pot = 0;
            Round.communityProgression = 1;

            player1ChipsLabel.Text = "Chips: " + Convert.ToString(Player1.chips);
            player2ChipsLabel.Text = "Chips: " + Convert.ToString(Player2.chips);

            potLabel.Text = "Pot: 0";
            
            player1RaiseLabel.Text = Convert.ToString(Player1.raiseAmount);
            player2RaiseLabel.Text = Convert.ToString(Player2.raiseAmount);
        }
        private void player1RaiseButton_Click(object sender, EventArgs e)
        {
            if (player1RaiseBox.Value > Player2.raiseAmount - Player1.raiseAmount)
            {
                int AmountToRaise = Convert.ToInt16(player1RaiseBox.Value);
                Player1.raiseAmount += AmountToRaise;
                player1RaiseLabel.Text = Convert.ToString(Player1.raiseAmount);

                Player1.chips -= AmountToRaise;
                player1ChipsLabel.Text = "Chips: " + Convert.ToString(Player1.chips);

                player2CallButton.Enabled = true; player2CheckButton.Enabled = false;
                player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;

                player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;
            }
        }
        private void player2RaiseButton_Click(object sender, EventArgs e)
        {
            if (player2RaiseBox.Value > Player1.raiseAmount - Player2.raiseAmount)
            {
                int AmountToRaise = Convert.ToInt16(player2RaiseBox.Value);
                Player2.raiseAmount += AmountToRaise;
                player2RaiseLabel.Text = Convert.ToString(Player2.raiseAmount);

                Player2.chips -= AmountToRaise;
                player2ChipsLabel.Text = "Chips: " + Convert.ToString(Player2.chips);

                player1CallButton.Enabled = true; player1CheckButton.Enabled = false;
                player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;

                player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;
            }
        }
        private void player1CheckButton_Click(object sender, EventArgs e)
        {
            Player1.check = true;
            if (Player2.check == true)
            {
                if (Round.communityProgression == 1)
                {
                    cCard1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                    cCard2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                    cCard3PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else if (Round.communityProgression == 2)
                {
                    cCard4PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else if (Round.communityProgression == 3)
                {
                    cCard5PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else
                {
                    HandDecider.Decide();

                    if (Player1.isWinner == true)
                    {
                        Player1.chips += Round.pot;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                    }
                    else if (Player2.isWinner == true)
                    {
                        Player2.chips += Round.pot;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                    }
                    else
                    {
                        Player1.chips += Round.pot / 2;
                        Player2.chips += Round.pot / 2;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                        player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                    }

                    Player1.isWinner = false;
                    Player2.isWinner = false;
                }

                Player2.check = false;
                Player1.check = false;
                
                if (Round.communityProgression != 4)
                {   
                    Round.communityProgression++;
                    
                    if (Round.bigBlindPosition == 1)
                    {
                        player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                        player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;

                        player2CallButton.Enabled = false; player2CheckButton.Enabled = true;
                        player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;
                    }
                    else
                    {
                        player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                        player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                        player1CallButton.Enabled = false; player1CheckButton.Enabled = true;
                        player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;
                    }
                }
                else
                {
                    StartNewRound();
                }
            }
            else
            {
                player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;

                player2CallButton.Enabled = false; player2CheckButton.Enabled = true;
                player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;
            }
        }
        private void player2CheckButton_Click(object sender, EventArgs e)
        {
            Player2.check = true;
            if (Player1.check == true)
            {
                if (Round.communityProgression == 1)
                {
                    cCard1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                    cCard2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                    cCard3PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else if (Round.communityProgression == 2)
                {
                    cCard4PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else if (Round.communityProgression == 3)
                {
                    cCard5PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                }
                else 
                {
                    HandDecider.Decide();

                    if (Player1.isWinner == true)
                    {
                        Player1.chips += Round.pot;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                    }
                    else if (Player2.isWinner == true)
                    {
                        Player2.chips += Round.pot;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                    }
                    else
                    {
                        Player1.chips += Round.pot / 2;
                        Player2.chips += Round.pot / 2;
                        Round.pot = 0;
                        potLabel.Text = "0";
                        player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                        player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                    }

                    Player1.isWinner = false; 
                    Player2.isWinner = false;
                }

                Player1.check = false;
                Player2.check = false;

                if (Round.communityProgression != 4)
                {
                    Round.communityProgression++;

                    if (Round.bigBlindPosition == 1)
                    {
                        player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                        player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;

                        player2CallButton.Enabled = false; player2CheckButton.Enabled = true;
                        player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;
                    }
                    else
                    {
                        player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                        player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                        player1CallButton.Enabled = false; player1CheckButton.Enabled = true;
                        player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;
                    }
                }
                else
                {
                    StartNewRound();
                }
                
            }
            else
            {
                player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                player1CallButton.Enabled = false; player1CheckButton.Enabled = true;
                player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;
            }
        }
        private void player1CallButton_Click(object sender, EventArgs e)
        {
            int amountToCall = Player2.raiseAmount - Player1.raiseAmount;
            Player1.chips -= amountToCall;
            Round.pot += 2 * Player2.raiseAmount;
            Player2.raiseAmount = 0;
            Player1.raiseAmount = 0;

            potLabel.Text = "Pot: " + Convert.ToString(Round.pot);

            player1RaiseLabel.Text = "0";
            player2RaiseLabel.Text = "0";
            player2ChipsLabel.Text = "Chips: " + Convert.ToString(Player2.chips);

            if (Round.communityProgression == 1)
            {
                cCard1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                cCard2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                cCard3PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else if (Round.communityProgression == 2)
            {
                cCard4PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else if (Round.communityProgression == 3)
            {
                cCard5PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else
            {
                HandDecider.Decide();
                if (Player1.isWinner == true)
                {
                    Player1.chips += Round.pot;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                }
                else if (Player2.isWinner == true)
                {
                    Player2.chips += Round.pot;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                }
                else
                {
                    Player1.chips += Round.pot / 2;
                    Player2.chips += Round.pot / 2;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                    player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                }

                Player1.isWinner = false;
                Player2.isWinner = false;
            }

            if (Round.communityProgression != 4)
            {
                Round.communityProgression++;

                if (Round.bigBlindPosition == 1)
                {
                    player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                    player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;

                    player2CallButton.Enabled = false; player2CheckButton.Enabled = true;
                    player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;
                }
                else
                {
                    player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                    player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                    player1CallButton.Enabled = false; player1CheckButton.Enabled = true;
                    player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;
                }
            }
            else
            {
                StartNewRound();
            }
        }
        private void player2CallButton_Click(object sender, EventArgs e)
        {
            int amountToCall = Player1.raiseAmount - Player2.raiseAmount;
            
            Player2.chips -= amountToCall;
            
            Round.pot += 2 * Player1.raiseAmount;
            
            Player1.raiseAmount = 0;
            Player2.raiseAmount = 0;

            potLabel.Text = "Pot: " + Convert.ToString(Round.pot);
            
            player1RaiseLabel.Text = "0";
            player2RaiseLabel.Text = "0";
            
            player2ChipsLabel.Text = "Chips: " + Convert.ToString(Player2.chips);

            if (Round.communityProgression == 1)
            {
                cCard1PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                cCard2PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
                cCard3PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else if (Round.communityProgression == 2)
            {
                cCard4PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else if (Round.communityProgression == 3)
            {
                cCard5PicBox.Image = Image.FromFile(Deck.paths[0]); Deck.paths.RemoveAt(0);
            }
            else
            {
                HandDecider.Decide();
                if (Player1.isWinner == true)
                {
                    Player1.chips += Round.pot;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                }
                else if (Player2.isWinner == true)
                {
                    Player2.chips += Round.pot;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                }
                else
                {
                    Player1.chips += Round.pot / 2;
                    Player2.chips += Round.pot / 2;
                    Round.pot = 0;
                    potLabel.Text = "0";
                    player1ChipsLabel.Text = Convert.ToString(Player1.chips);
                    player2ChipsLabel.Text = Convert.ToString(Player2.chips);
                }

                Player1.isWinner = false;
                Player2.isWinner = false;
            }

            if (Round.communityProgression != 4)
            {
                Round.communityProgression++;

                if (Round.bigBlindPosition == 1)
                {
                    player1CallButton.Enabled = false; player1CheckButton.Enabled = false;
                    player1FoldButton.Enabled = false; player1RaiseButton.Enabled = false;

                    player2CallButton.Enabled = false; player2CheckButton.Enabled = true;
                    player2FoldButton.Enabled = true; player2RaiseButton.Enabled = true;
                }
                else
                {
                    player2CallButton.Enabled = false; player2CheckButton.Enabled = false;
                    player2FoldButton.Enabled = false; player2RaiseButton.Enabled = false;

                    player1CallButton.Enabled = false; player1CheckButton.Enabled = true;
                    player1FoldButton.Enabled = true; player1RaiseButton.Enabled = true;
                }
            }
            else
            {
                StartNewRound();
            }
        }
        private void player1FoldButton_Click(object sender, EventArgs e)
        {
            int amountOnTable = Convert.ToInt16(player1RaiseLabel.Text);
            amountOnTable += Convert.ToInt16(player2RaiseLabel.Text);
            amountOnTable += Round.pot;
            
            Player2.chips += amountOnTable;

            player2RaiseLabel.Text = "0";
            player1RaiseLabel.Text = "0";
            
            potLabel.Text = "Pot: 0";
            
            player2ChipsLabel.Text = "Chips: " + Convert.ToString(Player2.chips);

            StartNewRound();
        }
        private void player2FoldButton_Click(object sender, EventArgs e)
        {
            int amountOnTable = Player1.raiseAmount;
            amountOnTable += Player2.raiseAmount;
            amountOnTable += Round.pot;
            
            Player1.chips += amountOnTable;
            
            player1RaiseLabel.Text = "0";
            player2RaiseLabel.Text = "0";
            
            potLabel.Text = "Pot: 0";
            
            player1ChipsLabel.Text = "Chips: " + Convert.ToString(Player1.chips);

            StartNewRound();
        }
        private void player1RaiseBox_ValueChanged(object sender, EventArgs e)
        {
            player1RaiseBox.Maximum = Player1.chips;
        }
        private void player2RaiseBox_ValueChanged(object sender, EventArgs e)
        {
            player2RaiseBox.Maximum = Player2.chips;
        }
        private void testButton_Click(object sender, EventArgs e)
        {
            List<string> playerhand = new List<string>() { "05_diamond", "07_club", "04_diamond", "06_diamond", "07_diamond", "8_diamond", "07_spade" };

            testBox.Text = playerhand[1].Substring(3, 1);
        }
        private void startFirstRoundButton_Click(object sender, EventArgs e)
        {
            StartNewRound();
        }
    }
}