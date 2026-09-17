using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        // How long to pause between each card being dealt, so a hand
        // appears one card at a time instead of all at once.
        private static readonly int CardDealDelayMs = 450;

        // BonusButton settings: how much it adds, how often it can be
        // used, and where a random ad image is picked from on a
        // successful claim. Ad images live under Assets/Ads and are
        // compiled straight into the exe as embedded resources (see the
        // csproj), so a published build needs no separate ads folder.
        private static readonly int BonusAmount = 100;
        private static readonly TimeSpan BonusCooldown = TimeSpan.FromMinutes(1);
        private const string BonusAdsResourcePrefix = "Fontys_ICT_block_1.Assets.Ads.";
        private static readonly string[] BonusAdImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        // Null until the first successful claim.
        private DateTime? lastBonusClaimTime;

        // Besides the manual BonusButton, the same ad also pops up on its
        // own every 3-5 finished rounds (re-randomized each time) - purely
        // an interruption, no bonus money attached.
        private static readonly Random RoundRandom = new Random();
        private int roundsSinceLastAd;
        private int nextAdAfterRounds = GetRandomAdInterval();

        private static int GetRandomAdInterval() => RoundRandom.Next(3, 6); // 3-5 inclusive

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

        // Adds BonusAmount to the balance, but only once every
        // BonusCooldown - clicking again before then just shows how much
        // longer is left. A successful claim also shows the bonus image
        // in an ad-style popup that can't be closed for a few seconds.
        private void BonusButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastBonusClaimTime != null)
            {
                TimeSpan sinceLastClaim = DateTime.Now - lastBonusClaimTime.Value;
                if (sinceLastClaim < BonusCooldown)
                {
                    TimeSpan remaining = BonusCooldown - sinceLastClaim;
                    OutputTextBlock.Text = $"Bonus available again in {remaining.Minutes}m {remaining.Seconds}s.";
                    return;
                }
            }

            lastBonusClaimTime = DateTime.Now;
            balance += BonusAmount;
            balanceTextBlock.Text = balance.ToString();
            OutputTextBlock.Text = string.Empty;

            ShowBonusAd();
        }

        // Shows the ad-style popup with BonusFilePath. Shared by the
        // manual BonusButton claim and the automatic every-3-to-5-rounds
        // trigger below.
        private void ShowBonusAd()
        {
            try
            {
                Stream adImageStream = GetRandomBonusAdStream();
                var adWindow = new BonusAdWindow(adImageStream) { Owner = this };
                adWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                OutputTextBlock.Text = $"Couldn't show the bonus image: {ex.Message}";
            }
        }

        // Picks a random ad image embedded in the assembly under
        // Assets/Ads, so it's a different ad each time instead of always
        // the same file - and works from a published exe with nothing
        // else needed alongside it.
        private Stream GetRandomBonusAdStream()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string[] resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(BonusAdsResourcePrefix) &&
                               BonusAdImageExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                .ToArray();

            if (resourceNames.Length == 0)
            {
                throw new InvalidOperationException("No ad images embedded under Assets/Ads.");
            }

            string chosenName = resourceNames[RoundRandom.Next(resourceNames.Length)];
            return assembly.GetManifestResourceStream(chosenName)
                ?? throw new InvalidOperationException($"Couldn't open embedded resource '{chosenName}'.");
        }

        // Call once per finished round (natural blackjack, bust/21 via
        // Hit, or Stand). Every 3-5 rounds, shows the same ad as
        // BonusButton, but without granting any money - just a periodic
        // interruption, like an ad-supported game.
        private void MaybeShowPeriodicAd()
        {
            roundsSinceLastAd++;
            if (roundsSinceLastAd < nextAdAfterRounds)
            {
                return;
            }

            roundsSinceLastAd = 0;
            nextAdAfterRounds = GetRandomAdInterval();
            ShowBonusAd();
        }

        // Adds whichever of playerCards aren't shown yet as animated
        // CardControls, one at a time with a short delay between each.
        // Only new cards are added - PlayerCardsPanel is cleared
        // separately at the start of a round - so repeated Hits don't
        // replay the animation for cards already on the table.
        private async Task RenderPlayerHand()
        {
            for (int i = PlayerCardsPanel.Children.Count; i < playerCards.Count; i++)
            {
                var cardControl = new CardControl(playerCards[i]);
                PlayerCardsPanel.Children.Add(cardControl);
                cardControl.PlayAppearAnimation();
                await Task.Delay(CardDealDelayMs);
            }
        }

        // Redraws the dealer's hand from scratch, one card at a time with
        // a short delay between each. While revealHoleCard is false, the
        // second card is shown face down and the running total is hidden,
        // since a real player wouldn't know it yet either.
        private async Task RenderDealerHand(bool revealHoleCard)
        {
            DealerCardsPanel.Children.Clear();
            for (int i = 0; i < dealerCards.Count; i++)
            {
                bool isHiddenHoleCard = !revealHoleCard && i == 1;
                CardControl cardControl = isHiddenHoleCard
                    ? CardControl.CreateFaceDown()
                    : new CardControl(dealerCards[i]);

                DealerCardsPanel.Children.Add(cardControl);
                cardControl.PlayAppearAnimation();
                await Task.Delay(CardDealDelayMs);
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

        private async void DealButton_Click(object sender, RoutedEventArgs e)
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
                PlayerCardsPanel.Children.Clear();
                (Card first, Card second) = Methods.Deal();
                playerCards.Add(first);
                playerCards.Add(second);

                int totalValue = playerCards.Sum(c => c.Value);
                await RenderPlayerHand();
                TotalValueTextBlock.Text = $"Total value: {totalValue}.";

                //give dealer the cards
                dealerCards.Clear();
                (Card dealerFirst, Card dealerSecond) = Methods.dealerCards();
                dealerCards.Add(dealerFirst);
                dealerCards.Add(dealerSecond);
                dealerTotalValue = dealerFirst.Value + dealerSecond.Value;

                // Hole card stays face down until the round is decided.
                await RenderDealerHand(revealHoleCard: false);

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
                    await RenderDealerHand(revealHoleCard: true);

                    var result = Methods.DefineWinner(totalValue, dealerTotalValue);
                    ResultTextBlock.Text = result;
                    // update balance and UI
                    balance = Methods.ChangeBalance.Update(balance, result, bet);
                    balanceTextBlock.Text = balance.ToString();
                    MaybeShowPeriodicAd();
                }
            }
        }

        private async void HitButton_Click(object sender, RoutedEventArgs e)
        {
            Card extraCard = Methods.ExtraCard();
            playerCards.Add(extraCard);

            int totalValue = playerCards.Sum(c => c.Value);
            await RenderPlayerHand();
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
                await RenderDealerHand(revealHoleCard: true);

                var result = Methods.DefineWinner(totalValue, dealerTotalValue);
                ResultTextBlock.Text = result;
                balance = Methods.ChangeBalance.Update(balance, result, currentBet);
                balanceTextBlock.Text = balance.ToString();
                MaybeShowPeriodicAd();
            }
        }

        private async void StandButton_Click(object sender, RoutedEventArgs e)
        {
            int totalValue = playerCards.Sum(c => c.Value);
            DealButton.IsEnabled = true;

            // Lock the round: without this, Hit/Stand stayed clickable
            // after the hand was already settled and paid out, letting the
            // player trigger another balance update on a finished hand.
            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;

            PlayDealerHand();
            await RenderDealerHand(revealHoleCard: true);

            var result = Methods.DefineWinner(totalValue, dealerTotalValue);
            ResultTextBlock.Text = result;
            balance = Methods.ChangeBalance.Update(balance, result, currentBet);
            balanceTextBlock.Text = balance.ToString();
            MaybeShowPeriodicAd();
        }
    }
}
