using Godot;
using System;
using System.Data;
using static Global;
using static NPCSpeechBubble;

public partial class NPC : CharacterBody3D
{
    public Hand hand;

    public DialogState dialogState;

    [Export]public Node3D model;

    [Export]public HandID handID;

    public ResourcePreloader resourcePreloader;
    [Export]public AnimationPlayer animPlayer;

#region Personality
    [Export]public Personality personality;
    public float confidence; // [0,1)
    public float anger; // [0,1)
#endregion
#region Ailments
    public SabotageProp distractedBy;
    public double distractionTimer;
    public double stunTimer;
    public double attackingTimer;

    public bool has21 = false;
    public bool busted = false;

    public int bet = 0;
#endregion
    [Signal] public delegate void GetDistractedEventHandler(SabotageProp prop);

    public enum Species
    {
        Fish,
        Seagull,
        Cat
    }
    [Export]public Species species;

    public enum Anims
    {
        Idle,
        Distracted,
        Dizzy,
        Attacking
    }

    [Export]public Node3D[] eyes;

    private float initialRot;

    public void PlayAnimation(Anims anim)
    {
        model.Position = Vector3.Zero;
        switch(anim)
        {
            case Anims.Idle:
                switch(species)
                {
                    case Species.Fish:    animPlayer.Play("Fish Anims/FishIdle"); break;
                    case Species.Seagull: animPlayer.Play("Seagull Anims/BirdIdle"); break;
                    case Species.Cat:     animPlayer.Play("Cat Anims/CatIdle"); break;
                }
                if(species != Species.Fish) model.Position = Vector3.Up*0.5f;
                break;
            case Anims.Distracted:
                switch(species)
                {
                    case Species.Fish:    animPlayer.Play("Fish Anims/FishIdle"); break;
                    case Species.Seagull: animPlayer.Play("Seagull Anims/BirdIdle"); break;
                    case Species.Cat:     animPlayer.Play("Cat Anims/CatIdle"); break;
                }
                if(species != Species.Fish) model.Position = Vector3.Up*0.5f;
                break;
            case Anims.Dizzy:
                switch(species)
                {
                    case Species.Fish:    animPlayer.Play("Fish Anims/FishDizzy"); break;
                    case Species.Seagull: animPlayer.Play("Seagull Anims/BirdDizzy"); break;
                    case Species.Cat:     animPlayer.Play("Cat Anims/CatDizzy"); break;
                }
                break;
            case Anims.Attacking:
                switch(species)
                {
                    case Species.Fish:    animPlayer.Play("Fish Anims/FishAttack"); break;
                    case Species.Seagull: animPlayer.Play("Seagull Anims/BirdAttack"); break;
                    case Species.Cat:     animPlayer.Play("Cat Anims/CatAttack"); break;
                }
                break;
        }
        if(anim == Anims.Dizzy)
        {
            eyes[0].Hide(); if(eyes[1] != null) eyes[1].Hide(); eyes[2].Show(); eyes[3].Show();
        } else
        {
            eyes[0].Show(); if(eyes[1] != null) eyes[1].Show(); eyes[2].Hide(); eyes[3].Hide();
        }
    }

    public override void _Ready()
    {
        hand = GetNode<Hand>("Hand");
        hand.handID = handID;
        hands[(int)handID] = hand;
        GetDistracted += Distract;
        resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
        SitAtTable();
        PlayAnimation(Anims.Idle);
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

    public void GetAngry(float angerMod)
    {
        anger += angerMod;
        anger = Mathf.Clamp(anger, 0, 1);

        SetDialogState(DialogState.Angry);
        // Roll for attack
        if(anger > 0.5f)
        {
            if(rng.Randf() <= anger)
            {
                SetDialogState(DialogState.Attacking);
                // ATTACK!!!
                GD.Print("Fuck you!!");
                switch(species)
                {
                    case Species.Fish: attackingTimer = 0.6; break;
                    case Species.Cat: attackingTimer = 0.6; break;
                    case Species.Seagull: attackingTimer = 0.6; break;
                }
                BlackjackHandler.Instance.freezeTimer = 3.0;
                PlayAnimation(Anims.Attacking);
            }
        }
    }

    public void SpawnAttack()
    {
        Attack atk = (resourcePreloader.GetResource("Attack") as PackedScene).Instantiate<Attack>();
        GetTree().CurrentScene.AddChild(atk);
        Vector3 targetPos = new Vector3(0, 2.3f, 0.25f);
        atk.spawner = this;
        atk.GlobalPosition = GlobalPosition + Vector3.Up*1.5f;
        atk.GlobalPosition += atk.GlobalPosition.DirectionTo(targetPos)*2.0f;
        atk.ApplyCentralImpulse(6.0f*atk.GlobalPosition.DirectionTo(targetPos));
        atk.ApplyTorqueImpulse(3.0f*atk.GlobalPosition.DirectionTo(targetPos));
        atk.GetNode<AudioStreamPlayer>("Whoosh").Play();
    }

    public override void _Process(double delta)
    {
        string side = handID == HandID.LeftNPC ? "Left" : (handID == HandID.MiddleNPC ? "Middle" : "Right");
        Node3D chipAnchor = GetTree().CurrentScene.GetNode("Chip Anchors").GetNode<Node3D>(side);
        if(bet == 0) chipAnchor.Hide();
        else
        {
            chipAnchor.Show();
            if(bet < 100)
            {
                chipAnchor.GetNode<Node3D>("large").Hide();
                chipAnchor.GetNode<Node3D>("medium").Hide();
                chipAnchor.GetNode<Node3D>("small").Show();
            } else if (bet < 150)
            {
                chipAnchor.GetNode<Node3D>("large").Hide();
                chipAnchor.GetNode<Node3D>("medium").Show();
                chipAnchor.GetNode<Node3D>("small").Hide();
            } else if (bet < 200)
            {
                chipAnchor.GetNode<Node3D>("large").Show();
                chipAnchor.GetNode<Node3D>("medium").Hide();
                chipAnchor.GetNode<Node3D>("small").Hide();
            } else
            {
                chipAnchor.GetNode<Node3D>("large").Show();
                chipAnchor.GetNode<Node3D>("medium").Show();
                chipAnchor.GetNode<Node3D>("small").Hide();
            }
        }

        if(distractionTimer > 0)
        {
            distractionTimer -= delta;
            if(distractionTimer <= 0)
            {
                distractionTimer = 0;
                distractedBy = null;
            }
        }

        // if(species == Species.Fish)
        // {
            float targetRot;
            if(distractionTimer > 0)
            {
                Vector2 targetPos = new Vector2(distractedBy.GlobalPosition.X, distractedBy.GlobalPosition.Z);
                Vector2 currentPos = new Vector2(GlobalPosition.X, GlobalPosition.Z);
                targetRot = Mathf.Atan2(targetPos.Y - currentPos.Y, targetPos.X - currentPos.X) + (species == Species.Fish ? Mathf.Pi*0.5f : -Mathf.Pi);
            } else
            {
                targetRot = initialRot;
            }
            // Rotation = new Vector3(0, AsymptoticApproach(Rotation.Y, targetRot, 20.0f*(float)delta), 0);
            Rotation = new Vector3(0, targetRot, 0);
        // }

        Scale = Vector3.One;
        if(stunTimer > 0)
        {
            stunTimer -= delta;
            Scale = new Vector3(1.0f, stunTimer > 0 ? 1.0f + 0.02f*Mathf.Sin((float)stunTimer) : 1.0f, 1.0f);
            if(stunTimer <= 0)
            {
                stunTimer = 0;
                GetAngry(0.3f);
                PlayAnimation(Anims.Idle);
            }
        }

        if(attackingTimer > 0)
        {
            attackingTimer -= delta;
            if(attackingTimer <= 0)
            {
                attackingTimer = 0;
                SpawnAttack();
                PlayAnimation(Anims.Idle);
            }
        }

        base._Process(delta);
    }

    public void OnBodyEntered(Node3D body)
    {
        if(body is SabotageProp)
        {
            SabotageProp s = body as SabotageProp;
            if(!s.stillEffective) return;
            GD.Print("Ow!");
            PlayAnimation(Anims.Dizzy);
            Camera.Instance.ShakeScreen(0.01f, 0.07);
            stunTimer = s.distractionBaseTime * 3.0f;
            // s.col.SetDeferred("disabled", true);
            (body as SabotageProp).LinearVelocity *= 4.0f;
            s.stillEffective = false;
            s.GetNode<AudioStreamPlayer>("Hit").Play();
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
        PlayAnimation(Anims.Distracted);
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

        return Mathf.Clamp(time * 2.5, 0.8f, 2.5f);
    }

    public bool HandOver()
    {
        return (has21 || isStanding || busted);
    }

    public bool IsAlert()
    {
        return stunTimer == 0 && distractionTimer == 0 && attackingTimer == 0;
    }

    public void SitAtTable()
    {
        // Put HAND ID assigning stuff here
        SetDialogState(DialogState.SittingDown);
        initialRot = Rotation.Y;
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
