using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Global : Node
{
	public static int money = 5000;
	public static int quota = 5500;
	public static bool screenShakeEnabled = true;
	public static float AsymptoticApproach(float current, float target, float multiplier, float addOn = 0.0f)
    {
        return current + ((target - current) * multiplier) + addOn;
    }

	public static Vector3 AsymptoticApproach(Vector3 current, Vector3 target, float multiplier, float addOn = 0.0f)
    {
        Vector3 dir = target - current;
        float val = (dir.Length() * multiplier) + addOn;
        return current + dir*val;
    }

    public static Color AsymptoticApproach(Color current, Color target, float multiplier, float addOn = 0.0f)
    {
        Vector3 currRGB = new Vector3(current.R, current.G, current.B);
        Vector3 targRGB = new Vector3(target.R, target.G, target.B);
        Vector3 move = AsymptoticApproach(currRGB, targRGB, multiplier);
        return new Color(
            move.X, move.Y, move.Z,
            current.A + ((target.A - current.A) * multiplier) + addOn);

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

	
	public static RandomNumberGenerator rng = new RandomNumberGenerator();

	public static PackedScene cardScene;

	public static List<Hand> hands = new List<Hand>(4){null, null, null, null};
	
	

	// public void DebugDealCards(Node parent)
    // {
    //     foreach(Node node in parent.GetChildren())
    //     {
    //         if(node is Hand)
    //         {
    //             Hand hand = node as Hand;
	// 			DealCard(hand, true);
    //         } else
    //         {
    //             DebugDealCards(node);
    //         }
    //     }
    // }

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

		cardScene = GD.Load<PackedScene>("res://entities/card/Card.tscn");

		// Hand dealerHand = GetTree().CurrentScene.GetNode("Player").GetNode<Hand>("Hand");

		// Hand debugNPCHand = GetTree().CurrentScene.GetNode("NPCs").GetNode("Fish NPC").GetNode<Hand>("Hand");

		// DealCard(dealerHand, true);
		// DealCard(dealerHand, false);

		// DealCard(debugNPCHand, true);
		// DealCard(debugNPCHand, true);

        base._Ready();
    }

    public override void _Process(double delta)
    {
		if(mouseClicked) {mouseClicked = false;}
		if(!mouseClicked && !mouseHeld && Input.IsMouseButtonPressed(MouseButton.Left)) {mouseClicked = true; mouseHeld = true;}
		if(!Input.IsMouseButtonPressed(MouseButton.Left)) {mouseHeld = false;}

        base._Process(delta);
    }


	public enum HandID
    {
        Dealer,
        LeftNPC,
        MiddleNPC,
        RightNPC
    };

	public enum Personality
    {
		Normal,
        Skittish,
		Lazy
    }

	public static bool mouseClicked;
	public static bool mouseHeld;
}
