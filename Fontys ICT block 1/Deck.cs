using System.Collections.Generic;

namespace Fontys_ICT_block_1
{
    // A single card: rank, suit and Blackjack value kept as separate
    // pieces of data instead of one combined "Ace of ♥" string, so the
    // UI can render each part (and color it) individually.
    public class Card
    {
        public string Rank { get; set; }
        public string Suit { get; set; }
        public int Value { get; set; }

        public Card(string rank, string suit, int value)
        {
            Rank = rank;
            Suit = suit;
            Value = value;
        }

        // Convenience for anywhere that just wants the old "Ace of ♥" text.
        public string Name => $"{Rank} of {Suit}";
    }

    // All 52 cards in a classic deck (excludes the Joker).
    // Suits use their symbol instead of the word: ♥ ♦ ♣ ♠
    // Values follow classic Blackjack rules:
    //   number cards  -> their number
    //   Jack/Queen/King -> 10
    //   Ace           -> 11
    public static class Deck
    {
        private static readonly string[] Suits = { "♥", "♦", "♣", "♠" };

        private static readonly (string Rank, int Value)[] Ranks =
        {
            ("Ace", 11), ("2", 2), ("3", 3), ("4", 4), ("5", 5), ("6", 6),
            ("7", 7), ("8", 8), ("9", 9), ("10", 10),
            ("Jack", 10), ("Queen", 10), ("King", 10)
        };

        // Cards are removed from this list as they're dealt, so call
        // Reset() whenever a new round starts to refill it back to 52.
        public static readonly List<Card> Cards = new List<Card>(52);

        public static void Reset()
        {
            Cards.Clear();
            Cards.AddRange(CreateFullDeck());
        }

        private static List<Card> CreateFullDeck()
        {
            var deck = new List<Card>();
            foreach (string suit in Suits)
            {
                foreach ((string rank, int value) in Ranks)
                {
                    deck.Add(new Card(rank, suit, value));
                }
            }
            return deck;
        }
    }
}
