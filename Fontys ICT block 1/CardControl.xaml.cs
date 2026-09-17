using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Fontys_ICT_block_1
{
    // Reusable visual for a single playing card. Used for both the
    // player's and the dealer's hand - create one per Card and add it to
    // whichever panel represents that hand, or use CreateFaceDown() for
    // the dealer's hidden hole card.
    public partial class CardControl : UserControl
    {
        public CardControl()
        {
            InitializeComponent();
        }

        public CardControl(Card card) : this()
        {
            SetCard(card);
        }

        public static CardControl CreateFaceDown()
        {
            var control = new CardControl();
            control.FrontFace.Visibility = Visibility.Collapsed;
            control.BackFace.Visibility = Visibility.Visible;
            return control;
        }

        public void SetCard(Card card)
        {
            FrontFace.Visibility = Visibility.Visible;
            BackFace.Visibility = Visibility.Collapsed;

            TopRankText.Text = card.Rank;
            BottomRankText.Text = card.Rank;
            SuitText.Text = card.Suit;

            bool isRed = card.Suit == "♥" || card.Suit == "♦";
            Brush suitColor = isRed ? Brushes.Crimson : Brushes.Black;
            TopRankText.Foreground = suitColor;
            BottomRankText.Foreground = suitColor;
            SuitText.Foreground = suitColor;
        }

        // Small "deal" animation: fades and scales the card in from
        // slightly smaller than full size. Call this right after adding
        // the card to its panel.
        public void PlayAppearAnimation()
        {
            Opacity = 0;
            var scale = new ScaleTransform(0.7, 0.7);
            RenderTransform = scale;
            RenderTransformOrigin = new Point(0.5, 0.5);

            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));

            var scaleUp = new DoubleAnimation(0.7, 1, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
        }
    }
}
