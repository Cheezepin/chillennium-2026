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
        foreach(NPC npc in npcs)
        {
            npc.Bet(dealerBJ);
        }
        ChangeState(BlackjackState.Dealing);
        AlertStatus("Dealing cards...", 2.0);
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
                npc.dialogState = DialogState.DoublingDown;
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
        int playerID = substate % 3;
        NPC npc = npcs[playerID];
        if(timer > npcs[playerID].ThinkingTime()) {
            timer = 0;
            if(!npc.HandOver() && npc.stunTimer <= 0 && npc.distractionTimer <= 0) {
                npc.MakeBJDecision();
                npc.CalculateConfidence();
            }
            ++substate;
            if(npcs[0].HandOver() && npcs[1].HandOver() && npcs[2].HandOver())
                ChangeState(BlackjackState.Conclusion);
        } else
        {
            // npcs[playerID].dialogState = DialogState.Thinking;
            if(npcs[playerID].confidence > 0.8f) npcs[playerID].dialogState = DialogState.ConfidentHand;
            else if(npcs[playerID].confidence < 0.3f) npcs[playerID].dialogState = DialogState.UnconfidentHand;
            else npcs[playerID].dialogState = DialogState.MiddlingHand;
        }
    }

    public void ConclusionState(double delta)
    {
        Hand dealerHand = hands[(int)HandID.Dealer];
        dealerHand.cards[1].showing = true; 
        timer += delta;
        switch(substate)
        {
            case 0:
                GD.Print("Game is over!!");
                ++substate;
                timer = 0;
                break;
            case 1:
                if(timer > 0.5)
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
                if(dealerHand.runningTotal == 21)
                {
                    GD.Print("Dealer win!! Take everyone's bet!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.dialogState = DialogState.Lose;
                        money += npc.bet;
                    }
                } else if(dealerHand.runningTotal > 21)
                {
                    GD.Print("Dealer bust!! Everyone wins their bet!!");
                    foreach(NPC npc in npcs)
                    {
                        npc.dialogState = DialogState.Win;
                        money -= npc.has21 ? (int)(npc.bet*1.5f) : npc.bet;
                    }
                } else
                {
                    foreach(NPC npc in npcs)
                    {
                        if(npc.has21)
                        {
                            npc.dialogState = DialogState.Blackjack;
                            GD.Print(npc.Name, " EPIC win!!");
                            money -= (int)(npc.bet*1.5f);
                        }
                        if(!npc.busted && npc.hand.runningTotal > dealerHand.runningTotal)
                        {
                            npc.dialogState = DialogState.Win;
                            GD.Print(npc.Name, " win!!");
                            money -= npc.bet;
                        } else
                        {
                            npc.dialogState = DialogState.Lose;
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

    public override void _Process(double delta)
    {
        switch(state)
        {
            case BlackjackState.Betting: BettingState(delta);       break;
            case BlackjackState.Dealing: DealState(delta);          break;
            case BlackjackState.Rounds:  RoundState(delta);         break;
            case BlackjackState.Conclusion: ConclusionState(delta); break;
            case BlackjackState.Idle:    IdleState(delta);          break;
        }

        if(Input.IsActionJustPressed("ui_down"))
        {
            ClearHands();
            ResetDeck();
            ShuffleDeck();
            ChangeState(BlackjackState.Betting);
        }
    }
}
