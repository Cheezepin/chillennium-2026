using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Global;
using static Deck;
using System.Data.SqlTypes;
using static UI;
using static NPCSpeechBubble;

public partial class BlackjackHandler : Node
{
    [Export]public Node NPCParent;
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

    public static BlackjackState state;
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
                if(timer > 0.8 || npc.isStanding)
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
                    timer -= 0.5;
                    if(dealerHand.runningTotal < 17)
                    {
                        DealCard(dealerHand, true);
                    } else
                    {
                        ++substate;
                    }
                }
                break;
            case 2:
                if(Player.Instance.frontHandHeld != null)
                {
                    Player.Instance.frontHandHeld.RemoveAnchorToHand();
                    Player.Instance.frontHandHeld = null;
                }
                if(dealerHand.runningTotal == 21)
                {
                    GD.Print("Dealer win!! Take everyone's bet!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.SetDialogState(DialogState.Lose);
                        money += npc.bet;
                    }
                } else if(dealerHand.runningTotal > 21)
                {
                    GD.Print("Dealer bust!! Everyone who hasn't busted wins their money!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.SetDialogState(DialogState.Win);
                        if(!npc.busted)
                            money -= npc.has21 ? (int)(npc.bet*1.5f) : npc.bet;
                    }
                } else
                {
                    foreach(NPC npc in npcs)
                    {
                        if(npc.has21)
                        {
                            npc.SetDialogState(DialogState.Blackjack);
                            GD.Print(npc.Name, " EPIC win!!");
                            money -= (int)(npc.bet*1.5f);
                        }
                        if(!npc.busted && npc.hand.runningTotal > dealerHand.runningTotal)
                        {
                            npc.SetDialogState(DialogState.Win);
                            GD.Print(npc.Name, " win!!");
                            money -= npc.bet;
                        } else
                        {
                            npc.SetDialogState(DialogState.Lose);
                            GD.Print(npc.Name, " lose!!");
                            money += npc.bet;
                        }
                    }
                }
                ChangeState(BlackjackState.Idle);
                break;
        }
    }

    public void IdleState(double delta)
    {
        
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

        if(Input.IsActionJustPressed("ui_down"))
        {
            dayMover.Play("day2sunset");
        }
        if(Input.IsActionJustPressed("ui_up"))
        {
            dayMover.Play("sunset2night");
        }
        if(Input.IsActionJustPressed("ui_left"))
        {
            dayMover.Play("RESET");
        }
    }

    public void InterruptBJ()
    {
        if(hands[0].runningTotal >= 21) ChangeState(BlackjackState.Conclusion);
    }
}
