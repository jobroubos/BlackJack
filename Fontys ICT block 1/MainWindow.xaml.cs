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

        // All cards currently in the dealer's hand. While the round is
        // still in progress, card[1] (the "hole card") is rendered face
        // down - see RenderDealerHand.
        private List<Card> dealerCards = new List<Card>();
        private int dealerTotalValue;

        int balance = 1000;

        // The bet that was actually checked against balance in
        // DealButton_Click, locked in for the whole round. Hit/Stand read
        // this instead of re-parsing NameTextBox.Text, so the amount used
        // to update the balance can't change mid-round or silently fail
        // to parse.
        int currentBet;

        public MainWindow()
        {

            InitializeComponent();
            // show initial balance
            balanceTextBlock.Text = balance.ToString();
        }

        // Blocks any character that isn't a digit before it even reaches
        // the TextBox, so NameTextBox only ever contains numbers.
        private void NameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // Redraws the player's hand as actual CardControl cards.
        private void RenderPlayerHand()
        {
            PlayerCardsPanel.Children.Clear();
            foreach (Card card in playerCards)
            {
                PlayerCardsPanel.Children.Add(new CardControl(card));
            }
        }

        // Redraws the dealer's hand. While revealHoleCard is false, the
        // second card is shown face down and the running total is hidden,
        // since a real player wouldn't know it yet either.
        private void RenderDealerHand(bool revealHoleCard)
        {
            DealerCardsPanel.Children.Clear();
            for (int i = 0; i < dealerCards.Count; i++)
            {
                bool isHiddenHoleCard = !revealHoleCard && i == 1;
                DealerCardsPanel.Children.Add(isHiddenHoleCard
                    ? CardControl.CreateFaceDown()
                    : new CardControl(dealerCards[i]));
            }

            DealerTotalValueTextBlock.Text = revealHoleCard
                ? $"Dealer's total value: {dealerTotalValue}."
                : string.Empty;
        }

        // Plays out the dealer's turn: keeps drawing while the dealer's
        // total is still under 17, adding each card to dealerCards and
        // updating the total. Call RenderDealerHand(true) afterwards to
        // show the result.
        private void PlayDealerHand()
        {
            Card? extraCard;
            while ((extraCard = Methods.DealerExtraCard(dealerTotalValue)) != null)
            {
                dealerCards.Add(extraCard);
                dealerTotalValue += extraCard.Value;
            }
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
                OutputTextBlock.Text = string.Empty;

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
                RenderPlayerHand();
                TotalValueTextBlock.Text = $"Total value: {totalValue}.";

                //give dealer the cards
                dealerCards.Clear();
                (Card dealerFirst, Card dealerSecond) = Methods.dealerCards();
                dealerCards.Add(dealerFirst);
                dealerCards.Add(dealerSecond);
                dealerTotalValue = dealerFirst.Value + dealerSecond.Value;

                // Hole card stays face down until the round is decided.
                RenderDealerHand(revealHoleCard: false);

                if (totalValue > 0)
                {
                    DealButton.IsEnabled = false;
                }

                // Either side getting a natural blackjack on the deal ends
                // the round immediately - the dealer doesn't get to draw
                // further cards in that case, only reveal what they have.
                if (totalValue == 21 || dealerTotalValue == 21)
                {
                    DealButton.IsEnabled = true;
                    HitButton.IsEnabled = false;
                    StandButton.IsEnabled = false;
                    RenderDealerHand(revealHoleCard: true);

                    var result = Methods.DefineWinner(totalValue, dealerTotalValue);
                    ResultTextBlock.Text = result;
                    // update balance and UI
                    balance = Methods.ChangeBalance.Update(balance, result, bet);
                    balanceTextBlock.Text = balance.ToString();
                }
            }
        }

        private void HitButton_Click(object sender, RoutedEventArgs e)
        {
            Card extraCard = Methods.ExtraCard();
            playerCards.Add(extraCard);

            int totalValue = playerCards.Sum(c => c.Value);
            RenderPlayerHand();
            TotalValueTextBlock.Text = $"Total value: {totalValue}.";

            // 21 or more (blackjack or bust) ends the round here.
            if (totalValue >= 21)
            {
                HitButton.IsEnabled = false;
                StandButton.IsEnabled = false;
                DealButton.IsEnabled = true;

                // A bust already loses regardless of the dealer's hand, so
                // there's no need for the dealer to draw any further -
                // just reveal what they've already got.
                if (totalValue <= 21)
                {
                    PlayDealerHand();
                }
                RenderDealerHand(revealHoleCard: true);

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
            RenderDealerHand(revealHoleCard: true);

            var result = Methods.DefineWinner(totalValue, dealerTotalValue);
            ResultTextBlock.Text = result;
            balance = Methods.ChangeBalance.Update(balance, result, currentBet);
            balanceTextBlock.Text = balance.ToString();
        }
    }
}
