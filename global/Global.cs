using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Global : Node
{
	public static float AsymptoticApproach(float current, float target, float multiplier)
    {
        return current + ((target - current) * multiplier);
    }

	public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
		Ten,
        Jack,
        Queen,
        King
    }

	public class CardValue
    {
        public Suit suit;
		public Rank rank;

		public CardValue(Suit s, Rank r) {suit=s;rank=r;}
    }

	public static List<CardValue> deck = new List<CardValue>();

	public static List<CardValue> regularDeck = new List<CardValue>()
    {
        new CardValue(Suit.Diamonds, Rank.Ace),
		new CardValue(Suit.Diamonds, Rank.Two),
		new CardValue(Suit.Diamonds, Rank.Three),
		new CardValue(Suit.Diamonds, Rank.Four),
		new CardValue(Suit.Diamonds, Rank.Five),
		new CardValue(Suit.Diamonds, Rank.Six),
		new CardValue(Suit.Diamonds, Rank.Seven),
		new CardValue(Suit.Diamonds, Rank.Eight),
		new CardValue(Suit.Diamonds, Rank.Nine),
		new CardValue(Suit.Diamonds, Rank.Ten),
		new CardValue(Suit.Diamonds, Rank.Jack),
		new CardValue(Suit.Diamonds, Rank.Queen),
		new CardValue(Suit.Diamonds, Rank.King),

		new CardValue(Suit.Hearts, Rank.Ace),
		new CardValue(Suit.Hearts, Rank.Two),
		new CardValue(Suit.Hearts, Rank.Three),
		new CardValue(Suit.Hearts, Rank.Four),
		new CardValue(Suit.Hearts, Rank.Five),
		new CardValue(Suit.Hearts, Rank.Six),
		new CardValue(Suit.Hearts, Rank.Seven),
		new CardValue(Suit.Hearts, Rank.Eight),
		new CardValue(Suit.Hearts, Rank.Nine),
		new CardValue(Suit.Hearts, Rank.Ten),
		new CardValue(Suit.Hearts, Rank.Jack),
		new CardValue(Suit.Hearts, Rank.Queen),
		new CardValue(Suit.Hearts, Rank.King),

		new CardValue(Suit.Clubs, Rank.Ace),
		new CardValue(Suit.Clubs, Rank.Two),
		new CardValue(Suit.Clubs, Rank.Three),
		new CardValue(Suit.Clubs, Rank.Four),
		new CardValue(Suit.Clubs, Rank.Five),
		new CardValue(Suit.Clubs, Rank.Six),
		new CardValue(Suit.Clubs, Rank.Seven),
		new CardValue(Suit.Clubs, Rank.Eight),
		new CardValue(Suit.Clubs, Rank.Nine),
		new CardValue(Suit.Clubs, Rank.Ten),
		new CardValue(Suit.Clubs, Rank.Jack),
		new CardValue(Suit.Clubs, Rank.Queen),
		new CardValue(Suit.Clubs, Rank.King),

		new CardValue(Suit.Spades, Rank.Ace),
		new CardValue(Suit.Spades, Rank.Two),
		new CardValue(Suit.Spades, Rank.Three),
		new CardValue(Suit.Spades, Rank.Four),
		new CardValue(Suit.Spades, Rank.Five),
		new CardValue(Suit.Spades, Rank.Six),
		new CardValue(Suit.Spades, Rank.Seven),
		new CardValue(Suit.Spades, Rank.Eight),
		new CardValue(Suit.Spades, Rank.Nine),
		new CardValue(Suit.Spades, Rank.Ten),
		new CardValue(Suit.Spades, Rank.Jack),
		new CardValue(Suit.Spades, Rank.Queen),
		new CardValue(Suit.Spades, Rank.King),
    };

	public static RandomNumberGenerator rng = new RandomNumberGenerator();

	public static PackedScene cardScene;
	
	public static void ResetDeck()
    {
		deck = regularDeck;
    }

	public static void ShuffleDeck()
    {
        List<CardValue> newDeck = new List<CardValue>();
        while(deck.Count != 0)
        {
            int randomIdx = rng.RandiRange(0, deck.Count - 1);
			newDeck.Add(deck[randomIdx]);
			deck.RemoveAt(randomIdx);
        }
		deck = newDeck;
    }

	public static (Suit, Rank) DrawCardValue()
    {
        int randomIdx = rng.RandiRange(0, deck.Count - 1);
		Suit s = deck[randomIdx].suit;
		Rank r = deck[randomIdx].rank;
		deck.RemoveAt(randomIdx);
		return (s, r);
    }

	public static void DealCard(Hand hand, bool show)
    {
        (Suit s, Rank r) = DrawCardValue();

		Card card = cardScene.Instantiate<Card>();
		card.suit = s;
		card.rank = r;
		card.showing = show;
		hand.AddChild(card);
    }

	public void DebugDealCards(Node parent)
    {
        foreach(Node node in parent.GetChildren())
        {
            if(node is Hand)
            {
                Hand hand = node as Hand;
				DealCard(hand, true);
            } else
            {
                DebugDealCards(node);
            }
        }
    }

	public static int GetValueOfRank(Rank rank)
    {
        switch(rank)
        {
            case Rank.Ace:
				return 11; // THIS IS IN THE CONTEXT OF INITIAL HAND CALC!!!!
			case Rank.Jack:
			case Rank.Queen:
			case Rank.King:
				return 10;
			default:
				return 1 + (int)rank;
        }
    }

	public static (int, bool) GetValueOfHand(List<Card> cardHand)
    {
        int sum = 0;
		List<int> vals = new List<int>();
		foreach(Card card in cardHand)
        {
            vals.Add(GetValueOfRank(card.rank));
        }
		sum = vals.Sum();
		if(sum > 21)
        {	
			for(int i = 0; i < vals.Count; ++i)
            {
                if(vals[i] == 11)
                {
                    vals[i] = 1;
					sum = vals.Sum();
					if(sum <= 21) return (sum, false);
                }
            }
        }
		
		return (sum, vals.Contains(11));
    }
	
    public override void _Ready()
    {
		rng.Randomize();
		ResetDeck();
		ShuffleDeck();

		cardScene = GD.Load<PackedScene>("res://entities/card/Card.tscn");

		Hand dealerHand = GetTree().CurrentScene.GetNode("Player").GetNode<Hand>("Hand");

		Hand debugNPCHand = GetTree().CurrentScene.GetNode("NPCs").GetNode("Fish NPC").GetNode<Hand>("Hand");

		DealCard(dealerHand, true);
		DealCard(dealerHand, false);

		DealCard(debugNPCHand, false);
		DealCard(debugNPCHand, false);

        base._Ready();
    }

}
