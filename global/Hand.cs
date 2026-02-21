using Godot;
using System;
using System.Collections.Generic;
using static Global;

public partial class Hand : Node3D
{
    public int runningTotal;
    public bool hasSoftAce; // true when there is an ace value of 11 being used

    public List<Card> cards = new List<Card>();

    public override void _Ready()
    {
        base._Ready();
        ChildEnteredTree += UpdateRunningTotal;

        foreach(Node node in GetChildren()) // probably debug but just to be safe, add cards on ready
        {
            if(node is Card && !cards.Contains((node as Card)))
            {
                cards.Add(node as Card);
            }
        }
        (runningTotal, hasSoftAce) = GetValueOfHand(cards);
    }

    public override void _Process(double delta)
    {   

        float count = (float)cards.Count;
        float centerOffset = 0.5f*(count-1);
        for(int i = 0; i < cards.Count; ++i)
        {
            Card card = cards[i];
            card.Position = 0.5f*new Vector3((float)i - centerOffset, 0, 0);
        }

        base._Process(delta);
    }

    public void UpdateRunningTotal(Node node)
    {
        if(node is Card)
        {
            Card card = node as Card;
            cards.Add(card);
            (runningTotal, hasSoftAce) = GetValueOfHand(cards);
        }
    }

    public void Hit()
    {
        DealCard(this, false);
    }
}
