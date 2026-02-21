using Godot;
using System;
using System.Data;
using static Global;
using static NPCSpeechBubble;

public partial class NPC : CharacterBody3D
{
    public Hand hand;

    public DialogState dialogState;

    [Export]public HandID handID;

#region Personality
    [Export]public Personality personality;
    public float confidence; // [0,1)
    public float anger; // [0,1)
#endregion
#region Ailments
    public SabotageProp distractedBy;
    public double distractionTimer;
    public double stunTimer;

    public bool has21 = false;
    public bool busted = false;

    public int bet = 0;
#endregion
    [Signal] public delegate void GetDistractedEventHandler(SabotageProp prop);

    public override void _Ready()
    {
        hand = GetNode<Hand>("Hand");
        hand.handID = handID;
        hands[(int)handID] = hand;
        GetDistracted += Distract;
        base._Ready();
    }

    public void CalculateConfidence()
    {
        if(hand.runningTotal == 21) {confidence = 0; return;} // this shouldn't even be called, but just in case
        // int dealerKnownValue = GetValueOfRank(hands[(int)HandID.Dealer].cards[0].rank);

        float conf;
        float handVal = (float)hand.runningTotal;
        switch(personality)
        {
            default:
            case Personality.Normal: 
                if(hand.runningTotal < 12)      {conf = 1.0f;}
                else if(hand.runningTotal < 17) {conf = 1.2f/Mathf.Log(handVal - 8.0f);}
                else                            {conf = 0.3f - 0.12f*(handVal - 17.0f);}
                break;
            case Personality.Skittish:
                if(hand.runningTotal < 12)      {conf = 1.0f;}
                else if(hand.runningTotal < 17) {conf = 0.4f - 0.01f*(handVal - 2.0f);}
                else                            {conf = 0.01f;}
                break;
            case Personality.Lazy:
                if(hand.runningTotal < 17)      {conf = 0.5f;}
                else                            {conf = 0.3f;}
                break;
        }

        if(hand.hasSoftAce) conf += 0.2f;

        confidence = Mathf.Clamp(conf, 0, 1.0f);
    }

    public bool isStanding = false;
    public void MakeBJDecision()
    {
        GD.Print("My total is ",hand.runningTotal," and my confidence is ",confidence);
        if(rng.Randfn(0, 0.999f) < confidence) // basically [0, 1)
        {
            SetDialogState(DialogState.Hit);
            GD.Print("I'm gonna hit!");
            // Hit
            hand.Hit();
        } else
        {
            SetDialogState(DialogState.Stand);
            // Stand
            GD.Print("I'm standing!");
            isStanding = true;
        }
    }

    public void ReactToBJDecision()
    {
        if(isStanding)
        {
            
        } else
        {
            if(hand.runningTotal == 21)
            {
                // Blackjack!
                SetDialogState(DialogState.Blackjack);
                GD.Print("I'm Rich?!");
            }
            if(hand.runningTotal > 21)
            {
                // Bust
                SetDialogState(DialogState.Bust);
                GD.Print("FUCK ME!!!!!");
            }
        }
    }

    public override void _Process(double delta)
    {

        if(Input.IsActionJustPressed("ui_left")) MakeBJDecision();

        if(distractionTimer > 0)
        {
            distractionTimer -= delta;
            if(distractionTimer <= 0)
            {
                distractionTimer = 0;
                distractedBy = null;
            }
        }

        base._Process(delta);
    }

    public void OnBodyEntered(Node3D body)
    {
        if(body is SabotageProp)
        {
            SabotageProp s = body as SabotageProp;
            GD.Print("Ow!");
            Camera.Instance.ShakeScreen(0.01f, 0.07);
            // s.col.SetDeferred("disabled", true);
            // (body as SabotageProp).LinearVelocity *= 10.0f;
        }
    }

    public void Distract(SabotageProp s)
    {
        distractedBy = s;
        double distractionModifier = 0;
        switch(personality)
        {
            case Personality.Skittish: distractionModifier = -0.4; break;
            case Personality.Lazy:     distractionModifier = 1.0; break;
        }
        distractionTimer = s.distractionBaseTime + distractionModifier;
        GD.Print(distractionTimer);
    }

    public void Bet(bool doubleExistingBet)
    {
        if(doubleExistingBet)
        {
            bet *= 2;
            GD.Print("Double or nothing! I bet ", bet);
            return;
        }
        switch(personality)
        {
            case Personality.Normal:   bet = 100; break;
            case Personality.Skittish: bet = rng.RandiRange(0, 15) < 1 ? rng.RandiRange(200, 250) : rng.RandiRange(50, 125); break;
            case Personality.Lazy:     bet = rng.RandiRange(20, 250); break;
        }
        SetDialogState(DialogState.Betting);
        GD.Print("Okay! I bet ",bet);
    }

    public double ThinkingTime()
    {
        if(HandOver()) return 0;
        double time = 1.0;
        time += 0.5 - confidence;

        switch(personality)
        {
            case Personality.Normal:                break;
            case Personality.Skittish: time -= 0.3; break;
            case Personality.Lazy:     time += 0.8; break;
        }

        return time * 2.5;
    }

    public bool HandOver()
    {
        return (has21 || isStanding || busted);
    }

    public void SitAtTable()
    {
        // Put HAND ID assigning stuff here
        SetDialogState(DialogState.SittingDown);
    }

    [Export]public SpeechBubbleSprite speechBubbleSprite;
    public void SetDialogState(DialogState newState)
    {
        if(dialogState != newState)
        {
            dialogState = newState;
            speechBubbleSprite.dialogTimer = 0;
        }
    }
}
