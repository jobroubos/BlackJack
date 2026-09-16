using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }
}
