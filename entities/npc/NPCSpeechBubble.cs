using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Global;

public partial class NPCSpeechBubble : Control
{
    // [Export]public HandID id;

    [Export]public NPC npc;

    public enum DialogState
    {
        SittingDown,
        Betting,
        DoublingDown,
        ConfidentHand,
        MiddlingHand,
        UnconfidentHand,
        Stand,
        Hit,
        Blackjack,
        Bust,
        Win,
        Lose,
        Angry,
        Attacking,
    }

    public List<Dictionary<DialogState, string>> dialogLists = new List<Dictionary<DialogState, string>>() {
        new Dictionary<DialogState, string>() { // Normal
            {DialogState.SittingDown,     "Alright! I'm pumped!"},
            {DialogState.Betting,         "I'll put down $!"},
            {DialogState.DoublingDown,    "Doubling down!! I'm throwing in $!"},
            {DialogState.ConfidentHand,   "I'm feeling pretty good about this one."},
            {DialogState.MiddlingHand,    "Hmm... I'm not sure how to feel right now."},
            {DialogState.UnconfidentHand, "This hand really isn't ideal..."},
            {DialogState.Stand,           "I'll stand!"},
            {DialogState.Hit,             "I'll hit!"},
            {DialogState.Blackjack,       "Yay!! A blackjack!"},
            {DialogState.Bust,            "Darn..."},
            {DialogState.Win,             "Woohoo!"},
            {DialogState.Lose,            "Aw, man..."},
            {DialogState.Angry,           "Grrr!!!!"},
            {DialogState.Attacking,       "Taste this, pal!"},
        },

        new Dictionary<DialogState, string>() { // Skittish
            {DialogState.SittingDown,     "I guess I'll try my hand at it…"},
            {DialogState.Betting,         "Um…I guess I'll try betting $"},
            {DialogState.DoublingDown,    "I-I'm doubling down for $!"},
            {DialogState.ConfidentHand,   "I think I might win!"},
            {DialogState.MiddlingHand,    "I can't tell whether to be happy or not"},
            {DialogState.UnconfidentHand, "Oh no, I don't think I'm gonna win"},
            {DialogState.Stand,           "I'll…stand…"},
            {DialogState.Hit,             "I think…I'm gonna…hit"},
            {DialogState.Blackjack,       "Wait, I got a blackjack!"},
            {DialogState.Bust,            "Aw…"},
            {DialogState.Win,             "I can't believe it, I won!"},
            {DialogState.Lose,            "Oh no!"},
            {DialogState.Angry,           "Hold on a minute!"},
            {DialogState.Attacking,       "why you little!"},
        },

        new Dictionary<DialogState, string>() { // Lazy
            {DialogState.SittingDown,     "Let's get this over with."},
            {DialogState.Betting,         "I'll bet $, I guess."},
            {DialogState.DoublingDown,    "Sure, I'll double down for $."},
            {DialogState.ConfidentHand,   "Ha, barely had to try."},
            {DialogState.MiddlingHand,    "..."},
            {DialogState.UnconfidentHand, "Tsk, darn."},
            {DialogState.Stand,           "Stand."},
            {DialogState.Hit,             "Hit."},
            {DialogState.Blackjack,       "Would you look at that, a blackjack."},
            {DialogState.Bust,            "Tsk…"},
            {DialogState.Win,             "Didn't even break a sweat."},
            {DialogState.Lose,            "What a scam."},
            {DialogState.Angry,           "Hey, watch it!"},
            {DialogState.Attacking,       "You want a piece of me?!"},
        }

    };

    private RichTextLabel TextBox;

    public override void _Ready()
    {
        TextBox = GetNode<RichTextLabel>("Text");
        base._Ready();
    }

    public override void _Process(double delta)
    {
        if(npc != null)
        {
            Show();

            string animName = "normal";
            if(npc.personality == Personality.Lazy) animName = "lazy";
            if(npc.personality == Personality.Skittish) animName = "skittish";
            GetNode<AnimatedSprite2D>("Base").Play(animName);

            TextBox.Text = dialogLists[(int)npc.personality][npc.dialogState];
            if((npc.dialogState == DialogState.Betting || npc.dialogState == DialogState.DoublingDown) && TextBox.Text.Contains('$'))
            {
                TextBox.Text = TextBox.Text.Insert(TextBox.Text.IndexOf('$')+1, npc.bet.ToString());
            }
        } else
        {
            Hide();
        }
        base._Process(delta);
    }

    
}
