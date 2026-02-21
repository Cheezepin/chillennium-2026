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
        Thinking,
        Stand,
        Hit,
        Blackjack,
        Bust,
        Win,
        Lose,
        Angry
    }

    public List<Dictionary<DialogState, string>> dialogLists = new List<Dictionary<DialogState, string>>() {
        new Dictionary<DialogState, string>() { // Normal
            {DialogState.SittingDown,     "Alright! I'm pumped!"},
            {DialogState.Betting,         "I'll put down $!"},
            {DialogState.DoublingDown,    "Doubling down!! I'm throwing in $!"},
            {DialogState.ConfidentHand,   "I'm feeling pretty good about this one."},
            {DialogState.MiddlingHand,    "Hmm... I can't tell what my odds are right now."},
            {DialogState.UnconfidentHand, "This hand really isn't ideal..."},
            {DialogState.Thinking,        "Let me think..."},
            {DialogState.Stand,           "I'll stand!"},
            {DialogState.Hit,             "I'll hit!"},
            {DialogState.Blackjack,       "Yay!! A blackjack!"},
            {DialogState.Bust,            "Darn..."},
            {DialogState.Win,             "Woohoo!"},
            {DialogState.Lose,            "Aw, man..."},
            {DialogState.Angry,           "Grrr!!!!"},
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

            TextBox.Text = dialogLists[0][npc.dialogState];
            if((npc.dialogState == DialogState.Betting || npc.dialogState == DialogState.DoublingDown) && TextBox.Text.Contains('$'))
            {
                TextBox.Text.Insert(TextBox.Text.IndexOf('$'), npc.bet.ToString());
            }
        } else
        {
            Hide();
        }
        base._Process(delta);
    }

}
