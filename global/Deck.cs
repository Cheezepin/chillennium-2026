using Godot;
using System;
using System.Collections.Generic;
using static Global;

public partial class Deck : Node3D
{
    public static Deck Instance;

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


    public static void ResetDeck()
    {
		deck = [.. regularDeck];
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
		card.GlobalPosition = Deck.Instance == null ? new Vector3(0.5f, 1.0f, -1.0f) : Deck.Instance.GlobalPosition;

        if(hand.handID == HandID.Dealer) card.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
    }

    public override void _Ready()
    {
        ResetDeck();
		ShuffleDeck();

        base._Ready();
    }

    public void DebugDealRound()
    {
        for(int i = 0; i < 4; ++i)
        {
            if(hands[i] != null)
            {
                DealCard(hands[i], true);
                DealCard(hands[i], i > 0);
            }
        }
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("ui_right")) DebugDealRound();
        base._Process(delta);
    }

}
