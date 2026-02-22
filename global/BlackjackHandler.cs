using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Global;
using static Deck;
using System.Data.SqlTypes;
using static UI;
using static NPCSpeechBubble;
using System.Security.Cryptography.X509Certificates;

public partial class BlackjackHandler : Node
{
    [Export]public Node NPCParent;
    [Export]public Node NPCAnchors;
    [Export]public ResourcePreloader fishResources;
    [Export]public ResourcePreloader seagullResources;
    [Export]public ResourcePreloader catResources;

    public int round = 0;
    public List<NPC> npcs = new List<NPC>();

    public static BlackjackHandler Instance;

    public enum BlackjackState
    {
        Betting,
        Dealing,
        Rounds,
        Conclusion,
        Idle,
    }

    public static BlackjackState state = BlackjackState.Idle;
    public int substate = 0;
    public double timer = 0;

    public bool dealerBJ = false;

    public int focusedPlayer = 0;

    [Export]public AnimationPlayer dayMover;

    public void GetNPCs()
    {
        npcs = [.. NPCParent.GetChildren().Cast<NPC>()]; // thanks vsc!
    }

    public override void _Ready()
    {
        Instance = this;
        GetNPCs();
        base._Ready();
    }

    public void ChangeState(BlackjackState newState)
    {
        state = newState;
        timer = 0;
        substate = 0;
    }

    public void BettingState(double delta)
    {
        switch(substate) {
            case 0:
                foreach(NPC npc in npcs)
                {
                    npc.Bet(dealerBJ);
                }
                timer = 0;
                ++substate;
                break;
            case 1:
                timer += delta;
                if(timer > 1.5) {
                    ChangeState(BlackjackState.Dealing);
                    AlertStatus("Dealing cards...", 2.0);
                }
                break;
        }
    }

    public bool HandleDealerBJ()
    {
        return hands[(int)HandID.Dealer].runningTotal == 21;
    }

    public void HandleBJDDs()
    {
        // PAUSE... ✋🤚
        for(int i = 0; i < 3; ++i)
        {
            NPC npc = npcs[i];
            while(npc.hand.runningTotal == 21)
            {
                npc.hand.DiscardCard(npc.hand.cards[1]);
                npc.hand.DiscardCard(npc.hand.cards[0]);
                DealCard(hands[i + 1], true);
                DealCard(hands[i + 1], true);
                npc.Bet(true);
                npc.SetDialogState(DialogState.DoublingDown);
            }
        }
    }

    public void ClearHands()
    {
        foreach(Hand hand in hands)
        {
            int count = hand.cards.Count;
            while(count > 0) {
                hand.DiscardCard(hand.cards[0]);
                --count;
            }
        }
    }

    public void DealState(double delta)
    {
        if(substate < 8)
        {
            timer -= delta;
            if(timer <= 0)
            {
                timer += 0.05;
                DealCard(hands[(substate + 1) % 4], substate != 7);
                ++substate;
            }
        } else if(substate == 8)
        {
            timer += delta;
            if(timer > 0.8) {
                if(HandleDealerBJ())
                {
                    GD.Print("Dealer win!");
                    dealerBJ = true;
                    ClearHands();
                    ResetDeck();
                    ShuffleDeck();
                    ChangeState(BlackjackState.Betting);
                } else {
                    ++substate;
                    timer = 0;
                    HandleBJDDs();
                    dealerBJ = false;
                }
            }
        } else if(substate == 9)
        {
            timer += delta;
            if(timer > 0.2)
            {
                foreach(NPC npc in npcs)
                {
                    npc.CalculateConfidence();
                    npc.isStanding = false;
                }
                ChangeState(BlackjackState.Rounds);
            }
        }
    }

    public void RoundState(double delta)
    {
        timer += delta;
        int playerID = (substate/3 % 3);
        NPC npc = npcs[playerID];
        switch(substate % 3) {
            case 0:
                if(npc.isStanding) {substate += 2; timer = 1.5;}
                if(timer > npcs[playerID].ThinkingTime()) {
                    if(!npc.HandOver() && npc.stunTimer <= 0 && npc.distractionTimer <= 0) {
                        npc.MakeBJDecision();
                        npc.CalculateConfidence();
                    } else
                    {
                        timer = 0;
                        substate += 3;
                    }
                    ++substate;
                    timer = 0;
                } else
                {
                    // npcs[playerID].SetDialogState(DialogState.Thinking;
                    if(npcs[playerID].confidence > 0.8f) npcs[playerID].SetDialogState(DialogState.ConfidentHand);
                    else if(npcs[playerID].confidence < 0.3f) npcs[playerID].SetDialogState(DialogState.UnconfidentHand);
                    else npcs[playerID].SetDialogState(DialogState.MiddlingHand);
                }
                break;
            case 1:
                if(timer > 0.8)
                {
                    npc.ReactToBJDecision();
                    ++substate;
                    timer = 0;
                }
                break;
            case 2:
                if(timer > 1.5)
                {
                    ++substate;
                    timer = 0;
                    if(npcs[0].HandOver() && npcs[1].HandOver() && npcs[2].HandOver())
                        ChangeState(BlackjackState.Conclusion);
                }
                break;
        }
    }

    public void ConclusionState(double delta)
    {
        Hand dealerHand = hands[(int)HandID.Dealer];
        // dealerHand.cards[1].showing = true; 
        timer += delta;
        switch(substate)
        {
            case 0:
                GD.Print("Game is over!!");
                AlertStatus("Round Over!", 2.0f);
                ++substate;
                timer = 0;
                foreach(Card c in dealerHand.cards) c.showing = true;
                break;
            case 1:
                if(timer < 1.7) break;
                if(timer > 2.2)
                {
                    timer -= 1.0;
                    if(dealerHand.runningTotal < 17)
                    {
                        DealCard(dealerHand, true);
                    } else
                    {
                        ++substate;
                        timer = 0;
                    }
                }
                break;
            case 2:
                int init = money;
                if(Player.Instance.frontHandHeld != null)
                {
                    Player.Instance.frontHandHeld.RemoveAnchorToHand();
                    Player.Instance.frontHandHeld = null;
                }
                int sum = 0;
                if(dealerHand.runningTotal == 21)
                {
                    GD.Print("Dealer win!! Take everyone's bet!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.SetDialogState(DialogState.Lose);
                        sum += npc.bet;
                    }
                    AlertStatus("You got a 21! You won $" + sum.ToString() + ".", 2.0);
                } else if(dealerHand.runningTotal > 21)
                {
                    GD.Print("Dealer bust!! Everyone who hasn't busted wins their money!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.SetDialogState(DialogState.Win);
                        if(!npc.busted)
                            sum -= npc.has21 ? (int)(npc.bet*1.5f) : npc.bet;
                    }
                    AlertStatus("You busted... You lost $" + Math.Abs(sum).ToString() + ".", 2.0);
                } else
                {
                    foreach(NPC npc in npcs)
                    {
                        if(npc.has21)
                        {
                            npc.SetDialogState(DialogState.Blackjack);
                            GD.Print(npc.Name, " EPIC win!!");
                            sum -= (int)(npc.bet*1.5f);
                        }
                        if(!npc.busted && npc.hand.runningTotal > dealerHand.runningTotal)
                        {
                            npc.SetDialogState(DialogState.Win);
                            GD.Print(npc.Name, " win!!");
                            sum -= npc.bet;
                        } else
                        {
                            npc.SetDialogState(DialogState.Lose);
                            GD.Print(npc.Name, " lose!!");
                            sum += npc.bet;
                        }

                        if(sum >= 0) AlertStatus("You earned $" + sum.ToString() + ".", 2.0);
                        else AlertStatus("You lost $" + Math.Abs(sum).ToString() + ".", 2.0);
                    }
                }
                money += sum;
                ++substate;
                timer = 0;
                switch(round)
                {
                    case 1: dayMover.Play("day2sunset"); break;
                    case 2: dayMover.Play("sunset2night"); break;
                    case 3: GetTree().ChangeSceneToFile("levels/EndScreen.tscn"); break;
                }

                GetNode<AudioStreamPlayer>(money > init ? "GOOD" : "BAD").Play();
                break;
            case 3:
                timer += delta;
                if(timer > 5.0) ChangeState(BlackjackState.Idle);
                break;
        }
    }

    public void IdleState(double delta)
    {
        if(timer == 0)
        {
            ++round;
            ClearHands();
            ResetDeck();
		    ShuffleDeck();

            foreach(NPC npc in npcs)
            {
                npc.QueueFree();
            }
            foreach(Node3D a in NPCAnchors.GetChildren())
            {
                if(a.GetChildCount() > 0) {
                    a.GetChild(0).QueueFree();
                    a.RemoveChild(a.GetChild(0));
                }
            }
            npcs.Clear();
            List<ResourcePreloader> loads = new List<ResourcePreloader>(){fishResources, seagullResources, catResources};
            HandID newID = HandID.LeftNPC;
            foreach(ResourcePreloader load in loads) {
                NPC newNPC = (load.GetResource(load.GetResourceList()[rng.RandiRange(0,load.GetResourceList().Length-1)]) as PackedScene).Instantiate<NPC>();
                newNPC.personality = (Personality)rng.RandiRange(0,2);
                newNPC.handID = newID;
                bool stillSearching = true;
                while(stillSearching)
                {
                    Node3D anchor = (NPCAnchors.GetChildren()[rng.RandiRange(0,2)] as Node3D);
                    if(anchor.GetChildCount() == 0) {
                        anchor.AddChild(newNPC);
                        newNPC.GlobalTransform = anchor.GlobalTransform;
                        newNPC.PlayAnimation(NPC.Anims.Idle);
                        stillSearching = false;
                    }
                }
                npcs.Add(newNPC);
                ++newID;
            }
        }
        timer += delta;
        if(timer > 6.0) ChangeState(BlackjackState.Betting);
    }

    public double freezeTimer = 0;
    public override void _Process(double delta)
    {
        if(freezeTimer > 0)
        {
            freezeTimer -= delta;
            if(freezeTimer < 0) freezeTimer = 0;
        } else {
            switch(state)
            {
                case BlackjackState.Betting: BettingState(delta);       break;
                case BlackjackState.Dealing: DealState(delta);          break;
                case BlackjackState.Rounds:  RoundState(delta);         break;
                case BlackjackState.Conclusion: ConclusionState(delta); break;
                case BlackjackState.Idle:    IdleState(delta);          break;
            }
        }

        // if(Input.IsActionJustPressed("ui_down"))
        // {
        //     ClearHands();
        //     ResetDeck();
        //     ShuffleDeck();
        //     ChangeState(BlackjackState.Betting);
        // }

        // if(Input.IsActionJustPressed("ui_down"))
        // {
        //     dayMover.Play("day2sunset");
        // }
        // if(Input.IsActionJustPressed("ui_up"))
        // {
        //     dayMover.Play("sunset2night");
        // }
        // if(Input.IsActionJustPressed("ui_left"))
        // {
        //     dayMover.Play("RESET");
        // }
    }

    public void InterruptBJ()
    {
        GD.Print(hands[0].runningTotal);
        if(hands[0].runningTotal >= 21) 
                ChangeState(BlackjackState.Conclusion);
    }
}
