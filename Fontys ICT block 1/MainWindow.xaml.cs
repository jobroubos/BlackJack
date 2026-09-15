using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Fontys_ICT_block_1
{

    public partial class MainWindow : Window
    {
        // All cards currently in the player's hand (starting two, plus any hits).
        private List<Card> playerCards = new List<Card>();
        private List<Card> dealerCards = new List<Card>();
        private Card? dealerCard1;
        private Card? dealerCard2;
        private int dealerTotalValue;


        public MainWindow()
        {

            InitializeComponent();
            // show initial balance
            balanceTextBlock.Text = balance.ToString();
        }

        int balance = 1000;

        // The bet that was actually checked against balance in
        // DealButton_Click, locked in for the whole round. Hit/Stand read
        // this instead of re-parsing NameTextBox.Text, so the amount used
        // to update the balance can't change mid-round or silently fail
        // to parse.
        int currentBet;

        // Blocks any character that isn't a digit before it even reaches
        // the TextBox, so NameTextBox only ever contains numbers.
        private void NameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // Plays out the dealer's turn: keeps drawing while
        // Methods.DealerExtraCard says the dealer's total is still under
        // 17, adding each card to dealerCards and updating the total, then
        // refreshes the dealer TextBlocks. Without the loop and without
        // actually using the returned Card, DealerExtraCard's result was
        // just being discarded and the dealer never grew a hand.
        private void PlayDealerHand()
        {
            Card? extraCard;
            while ((extraCard = Methods.DealerExtraCard(dealerTotalValue)) != null)
            {
                dealerCards.Add(extraCard);
                dealerTotalValue += extraCard.Value;
            }

            DealerTextBlock.Text = $"Dealer's cards: {string.Join(", ", dealerCards.Select(c => c.Name))}.";
            DealerTotalValueTextBlock.Text = $"Dealer's total value: {dealerTotalValue}.";
        }

        private void DealButton_Click(object sender, RoutedEventArgs e)
        {

            string name = NameTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                OutputTextBlock.Text = "Please input an amount.";
                return;
            }

            if (!int.TryParse(name, out int bet))
            {
                OutputTextBlock.Text = "Please input a valid amount.";
                return;
            }

            // A bet of 0 risks nothing, so require a real amount.
            if (bet <= 0)
            {
                OutputTextBlock.Text = "Please enter a bet greater than 0.";
                return;
            }

            if (bet > balance)
            {
                OutputTextBlock.Text = "You don't have enough balance.";
                return;
            }

            // valid bet, proceed
            else
            {
                // Lock this bet in for the round - Hit/Stand use
                // currentBet from here on, not the textbox.
                currentBet = bet;

                // Refill the deck back to 52 cards so previous rounds'
                // removals don't leave us short later on.
                Deck.Reset();
                HitButton.IsEnabled = true;
                StandButton.IsEnabled = true;
                ResultTextBlock.Text = string.Empty;
                // Deal() gives back two Card objects bundled together.
                // Start a fresh hand so a previous round's hits don't carry over.
                playerCards.Clear();
                (Card first, Card second) = Methods.Deal();
                playerCards.Add(first);
                playerCards.Add(second);

                int totalValue = playerCards.Sum(c => c.Value);
                OutputTextBlock.Text = $"Your cards: {string.Join(", ", playerCards.Select(c => c.Name))}.";
                TotalValueTextBlock.Text = $"Total value: {totalValue}.";

                //give dealer the cards
                
                
                dealerCards.Clear();
                (dealerCard1, dealerCard2) = Methods.dealerCards();
                dealerCards.Add(dealerCard1);
                dealerCards.Add(dealerCard2);
                dealerTotalValue = dealerCard1.Value + dealerCard2.Value;
                DealerTextBlock.Text = $"Dealer's cards: {dealerCard1.Name} and {dealerCard2.Name}.";
                DealerTotalValueTextBlock.Text = $"Dealer's total value: {dealerTotalValue}.";

                // The winner isn't decided until the player stands.
                ResultTextBlock.Text = string.Empty;

                if (totalValue > 0)
                {
                    DealButton.IsEnabled = false;
                }
                if (totalValue == 21)
                {
                    DealButton.IsEnabled = true;
                    PlayDealerHand();
                    var result = Methods.DefineWinner(totalValue, dealerTotalValue);
                    ResultTextBlock.Text = result;
                    // update balance and UI
                    balance = Methods.ChangeBalance.Update(balance, result, bet);
                    balanceTextBlock.Text = balance.ToString();
                    TotalValueTextBlock.Text = string.Empty;
                    OutputTextBlock.Text = string.Empty;
                }
            }
        }

        private void HitButton_Click(object sender, RoutedEventArgs e)
        {


            Card extraCard = Methods.ExtraCard();
            playerCards.Add(extraCard);

            int totalValue = playerCards.Sum(c => c.Value);
            OutputTextBlock.Text = $"Your cards: {string.Join(", ", playerCards.Select(c => c.Name))}.";
            TotalValueTextBlock.Text = $"Total value: {totalValue}.";
            // totalValue == 21 and totalValue > 21 are both already covered
            // by >= 21, so this used to be three separate ifs that all
            // fired together whenever the round ended here - applying the
            // balance change two or three times over instead of once. One
            // if for "the round is over" fixes that.
            if (totalValue >= 21)
            {
                HitButton.IsEnabled = false;
                StandButton.IsEnabled = false;
                DealButton.IsEnabled = true;
                PlayDealerHand();
                var result = Methods.DefineWinner(totalValue, dealerTotalValue);
                ResultTextBlock.Text = result;
                balance = Methods.ChangeBalance.Update(balance, result, currentBet);
                balanceTextBlock.Text = balance.ToString();
            }
        }

        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            int totalValue = playerCards.Sum(c => c.Value);
            DealButton.IsEnabled = true;

            // Lock the round: without this, Hit/Stand stayed clickable
            // after the hand was already settled and paid out, letting the
            // player trigger another balance update on a finished hand.
            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;
            PlayDealerHand();
            var result = Methods.DefineWinner(totalValue, dealerTotalValue);
            ResultTextBlock.Text = result;
            balance = Methods.ChangeBalance.Update(balance, result, currentBet);
            balanceTextBlock.Text = balance.ToString();
            TotalValueTextBlock.Text = string.Empty;
            OutputTextBlock.Text = string.Empty;


        }
    }
}
