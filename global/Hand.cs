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
    float targetOutlineWidth = 0.9f;

    public override void _Process(double delta)
    {   

        float count = (float)cards.Count;
        float centerOffset = 0.5f*(count-1);
        for(int i = 0; i < cards.Count; ++i)
        {
            Card card = cards[i];
            float val = (float)i - centerOffset;
            Vector3 cardTargetPos;
            if(handID != HandID.Dealer) {
                Vector3 cardTargetRot  = new Vector3(card.Rotation.X, card.Rotation.Y, -0.3f*val);
                card.Rotation = AsymptoticApproach(card.Rotation, cardTargetRot, 10.0f*(float)delta);
                cardTargetPos = new Vector3(0.3f*val, -val*val*0.01f, Mathf.Abs(val)*-0.01f);
            } else
            {
                cardTargetPos = new Vector3(0.35f*val, 0, 0);
            }
            card.Position = AsymptoticApproach(card.Position, cardTargetPos, 
                Mathf.Clamp(0.1f / (card.Position - cardTargetPos).LengthSquared(), 20.0f, 200.0f)*(float)delta);

            card.useHandColor = (Player.Instance.frontHandHeld != this);

            // if(!card.useHandColor) card.Position = cardTargetPos;
            if(Player.Instance.frontHandHeld != this) {
                card.front.Modulate = AsymptoticApproach(card.front.Modulate, targetModulate, 40.0f*(float)delta);
                card.targetOutlineWidth = targetOutlineWidth;
            }
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
        cardSelectBox.Size = new Vector3((float)cards.Count*0.32f, cardSelectBox.Size.Y, cardSelectBox.Size.Z);

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
        Vector3 cardPos = card.GlobalPosition;
        DiscardCard(card);
        Discard.Instance.AddChild(card);
        card.GlobalPosition = cardPos;
        DealCard(this, true);
        Card justDealtCard = cards.Last(); // whatever man fuck my stupid chud life
        cards.Remove(justDealtCard);
        cards.Insert(cardIdx, justDealtCard);
        justDealtCard.GlobalPosition = Deck.Instance.GlobalPosition;
    }

    public void OnMouseEnter()
    {
        if(handID == HandID.Dealer && cards.Count > 1) 
        {
            if(BlackjackHandler.state < BlackjackHandler.BlackjackState.Conclusion) cards[1].showing = true;
        }
        else
        {
            targetModulate = new Color(0.5f, 0.5f, 0.7f);
            targetOutlineWidth = 1.2f;
        }
    }
	
	public void OnMouseExit()
    {
        if(handID == HandID.Dealer && cards.Count > 1) 
        {
           if(BlackjackHandler.state < BlackjackHandler.BlackjackState.Conclusion) cards[1].showing = false;
        }
        else
        {
            targetModulate = new Color(1.0f, 1.0f, 1.0f);
            targetOutlineWidth = 0.9f;
        }
    }

	public void AnchorToHand()
    {
        if(Player.Instance.frontHandHeld != null) Player.Instance.frontHandHeld.RemoveAnchorToHand();
        Player.Instance.frontHandHeld = this;
        targetModulate = new Color(1.0f, 1.0f, 1.0f);
        hitbox.SetDeferred("disabled", true);
		targetOutlineWidth = 0.9f;
    }

    public void RemoveAnchorToHand()
    {
        Player.Instance.frontHandHeld = null;
        targetModulate = new Color(1.0f, 1.0f, 1.0f);
        hitbox.SetDeferred("disabled", false);
		targetOutlineWidth = 0.9f;
    }

	public void OnInputEvent(Node cam, InputEvent inputEvent, Vector3 eventPosition, Vector3 normal, int shapeIdx) {
		if(inputEvent is InputEventMouseButton)
        {
            if(((inputEvent as InputEventMouseButton).ButtonMask & MouseButtonMask.Left) != 0)
            {
                if(BlackjackHandler.state == BlackjackHandler.BlackjackState.Rounds || BlackjackHandler.state == BlackjackHandler.BlackjackState.Conclusion)
                    AnchorToHand();
            }
        }
	}
}
