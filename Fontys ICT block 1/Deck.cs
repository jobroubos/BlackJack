using System;
using System.Collections.Generic;
using System.Text;

namespace Fontys_ICT_block_1
{
    // A single card: its display name and its point value.
    // Keeping this as its own small class means every card
    // carries both pieces of info together.
    public class Card
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public Card(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    // All 52 cards in a classic deck, written out one by one
    // (excludes the Joker).
    // Suits use their symbol instead of the word: ♥ ♦ ♣ ♠
    // Values follow classic Blackjack rules:
    //   number cards  -> their number
    //   Jack/Queen/King -> 10
    //   Ace           -> 11
    public static class Deck
    {
        // Cards are removed from this list as they're dealt, so call
        // Reset() whenever a new round starts to refill it back to 52.
        public static readonly List<Card> Cards = new List<Card>(52);

        public static void Reset()
        {
            Cards.Clear();
            Cards.AddRange(CreateFullDeck());
        }

        private static List<Card> CreateFullDeck() => new List<Card>
        {
            new Card("Ace of ♥", 11),
            new Card("2 of ♥", 2),
            new Card("3 of ♥", 3),
            new Card("4 of ♥", 4),
            new Card("5 of ♥", 5),
            new Card("6 of ♥", 6),
            new Card("7 of ♥", 7),
            new Card("8 of ♥", 8),
            new Card("9 of ♥", 9),
            new Card("10 of ♥", 10),
            new Card("Jack of ♥", 10),
            new Card("Queen of ♥", 10),
            new Card("King of ♥", 10),
            new Card("Ace of ♦", 11),
            new Card("2 of ♦", 2),
            new Card("3 of ♦", 3),
            new Card("4 of ♦", 4),
            new Card("5 of ♦", 5),
            new Card("6 of ♦", 6),
            new Card("7 of ♦", 7),
            new Card("8 of ♦", 8),
            new Card("9 of ♦", 9),
            new Card("10 of ♦", 10),
            new Card("Jack of ♦", 10),
            new Card("Queen of ♦", 10),
            new Card("King of ♦", 10),
            new Card("Ace of ♣", 11),
            new Card("2 of ♣", 2),
            new Card("3 of ♣", 3),
            new Card("4 of ♣", 4),
            new Card("5 of ♣", 5),
            new Card("6 of ♣", 6),
            new Card("7 of ♣", 7),
            new Card("8 of ♣", 8),
            new Card("9 of ♣", 9),
            new Card("10 of ♣", 10),
            new Card("Jack of ♣", 10),
            new Card("Queen of ♣", 10),
            new Card("King of ♣", 10),
            new Card("Ace of ♠", 11),
            new Card("2 of ♠", 2),
            new Card("3 of ♠", 3),
            new Card("4 of ♠", 4),
            new Card("5 of ♠", 5),
            new Card("6 of ♠", 6),
            new Card("7 of ♠", 7),
            new Card("8 of ♠", 8),
            new Card("9 of ♠", 9),
            new Card("10 of ♠", 10),
            new Card("Jack of ♠", 10),
            new Card("Queen of ♠", 10),
            new Card("King of ♠", 10)
        };
    }
}
