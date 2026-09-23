using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
//this is a test for www.jobroubos.nl/github
namespace Fontys_ICT_block_1
{
    public static class Methods
    {
        public static class ChangeBalance
        {
            public static int Update(int balance, string result, int bet)
            {
                // Adjust the balance based on the round result. These
                // strings must match DefineWinner's return values exactly -
                // if DefineWinner's wording ever changes, update it here too.
                if (result == "You win!")
                {
                    balance += bet;
                }
                else if (result == "Dealer wins!")
                {
                    balance -= bet;
                }
                // It's a tie -> no change
                return balance;
            }
        }


        public static (Card, Card) Deal()
        {

            Random rnd = new Random();
            Card card1 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
            Deck.Cards.Remove(card1);
            Card card2 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
            Deck.Cards.Remove(card2);
            return (card1, card2);

        }

        public static Card ExtraCard()
        {
            Random rnd = new Random();
            Card ExtraCard1 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
            Deck.Cards.Remove(ExtraCard1);
            return (ExtraCard1);

        }
        
        public static Card? DealerExtraCard(int dealerTotal)
        {
            // Guard clause: only deal a card if dealer's total is over 17
            if (dealerTotal < 17)
            {
                Random rnd = new Random();
                Card DealerExtraCard1 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
                Deck.Cards.Remove(DealerExtraCard1);
                return DealerExtraCard1;
            }

            return null; // no card dealt
        }

        public static (Card, Card) dealerCards()
        {
            Random rnd = new Random();
            Card dealerCard1 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
            Deck.Cards.Remove(dealerCard1);
            Card dealerCard2 = Deck.Cards[rnd.Next(Deck.Cards.Count)];
            Deck.Cards.Remove(dealerCard2);
            return (dealerCard1, dealerCard2);
        }
        // Takes the two totals as input and hands back a plain string
        // describing the result. Methods.cs has no idea a ResultTextBlock
        // even exists - that's MainWindow's job to display.
        public static string DefineWinner(int totalValue, int dealerTotalValue)
        {
            var result = string.Empty;
            if (totalValue > 21 || (dealerTotalValue <= 21 && totalValue < dealerTotalValue))
            {
                result = "Dealer wins!";
                return "Dealer wins!";
            }
            else if (dealerTotalValue > 21 || totalValue > dealerTotalValue)
            {
                result = "You win!";
                return "You win!";
            }
            else
            {
                result = "It's a tie!";
                return "It's a tie!";

            }
        }
        
    }
}
