using Godot;
using System;
using System.Collections.Generic;
using static Global;
using static Deck;
using System.Linq;
public partial class Hand : Node3D
{
    [Export]public HandID handID;
    public int runningTotal;
    public bool hasSoftAce; // true when there is an ace value of 11 being used

    public CollisionShape3D hitbox;

    public List<Card> cards = new List<Card>();

    Transform3D home;

    public NPC parent;

    public override void _Ready()
    {
        base._Ready();
        ChildEnteredTree += AddCardToHand;

        foreach(Node node in GetChildren()) // probably debug but just to be safe, add cards on ready
        {
            if(node is Card && !cards.Contains((node as Card)))
            {
                cards.Add(node as Card);
            }
        }
        (runningTotal, hasSoftAce) = GetValueOfHand(cards);

        home = GlobalTransform;

        if(GetParent() is not NPC) 
            hands[(int)handID] = this;
        else
            parent = GetParent<NPC>();
        
        hitbox = GetChild(0).GetChild<CollisionShape3D>(0);


    }

    Color targetModulate = new Color(1,1,1);

    public override void _Process(double delta)
    {   

        float count = (float)cards.Count;
        float centerOffset = 0.5f*(count-1);
        for(int i = 0; i < cards.Count; ++i)
        {
            Card card = cards[i];
            Vector3 cardTargetPos = 0.4f*new Vector3((float)i - centerOffset, 0, 0);
            card.Position = AsymptoticApproach(card.Position, cardTargetPos, 10.0f*(float)delta);

            card.useHandColor = (Player.Instance.frontHandHeld != this);
            if(Player.Instance.frontHandHeld != this) card.front.Modulate = targetModulate;
        }

        GlobalTransform = (Player.Instance.frontHandHeld == this) ? Player.Instance.frontAnchor.GlobalTransform : home;

        base._Process(delta);
    }

    public void AddCardToHand(Node node)
    {
        if(node is Card)
        {
            Card card = node as Card;
            cards.Add(card);
            (runningTotal, hasSoftAce) = GetValueOfHand(cards);
        }

        BoxShape3D cardSelectBox = GetChild(0).GetChild<CollisionShape3D>(0).Shape as BoxShape3D;
        cardSelectBox.Size = new Vector3((float)cards.Count*0.4f, cardSelectBox.Size.Y, cardSelectBox.Size.Z);

        if(parent != null)
        {
            parent.has21 = runningTotal == 21;
            parent.busted = runningTotal > 21;
        }
    }

    public void Hit()
    {
        DealCard(this, true);
    }

    public void DiscardCard(Card card)
    {
        RemoveChild(card);
        cards.Remove(card);
        (runningTotal, hasSoftAce) = GetValueOfHand(cards);

        BoxShape3D cardSelectBox = GetChild(0).GetChild<CollisionShape3D>(0).Shape as BoxShape3D;
        cardSelectBox.Size = new Vector3((float)cards.Count*0.5f, cardSelectBox.Size.Y, cardSelectBox.Size.Z);
    }

    public void SwapCard(Card card)
    {
        if(deck.Count == 0) return;
        int cardIdx = cards.IndexOf(card);
        DiscardCard(card);
        Discard.Instance.AddChild(card);
        DealCard(this, true);
        Card justDealtCard = cards.Last(); // whatever man fuck my stupid chud life
        cards.Remove(justDealtCard);
        cards.Insert(cardIdx, justDealtCard);
    }

    public void OnMouseEnter()
    {
		// targetOutlineWidth = 1.2f;
        if(handID == HandID.Dealer && cards.Count > 1) 
        {
            if(BlackjackHandler.state < BlackjackHandler.BlackjackState.Conclusion) cards[1].showing = true;
        }
        else targetModulate = new Color(1.0f, 1.0f, 0.0f);
    }
	
	public void OnMouseExit()
    {
        // targetOutlineWidth = 0.9f;
        if(handID == HandID.Dealer && cards.Count > 1) 
        {
           if(BlackjackHandler.state < BlackjackHandler.BlackjackState.Conclusion) cards[1].showing = false;
        }
        else targetModulate = new Color(1.0f, 1.0f, 1.0f);
    }

	public void AnchorToHand()
    {
        if(Player.Instance.frontHandHeld != null) Player.Instance.frontHandHeld.RemoveAnchorToHand();
        Player.Instance.frontHandHeld = this;
        targetModulate = new Color(1.0f, 1.0f, 1.0f);
        hitbox.SetDeferred("disabled", true);
		// targetOutlineWidth = 0.9f;
    }

    public void RemoveAnchorToHand()
    {
        Player.Instance.frontHandHeld = null;
        targetModulate = new Color(1.0f, 1.0f, 1.0f);
        hitbox.SetDeferred("disabled", false);
		// targetOutlineWidth = 0.9f;
    }

	public void OnInputEvent(Node cam, InputEvent inputEvent, Vector3 eventPosition, Vector3 normal, int shapeIdx) {
		if(inputEvent is InputEventMouseButton)
        {
            if(((inputEvent as InputEventMouseButton).ButtonMask & MouseButtonMask.Left) != 0)
            {
                AnchorToHand();
            }
        }
	}
}
