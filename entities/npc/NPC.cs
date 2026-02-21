using Godot;
using System;

public partial class NPC : CharacterBody3D
{
    public Hand hand;
    public override void _Ready()
    {
        hand = GetNode<Hand>("Hand");
        base._Ready();
    }

    public void MakeBJDecision()
    {
        if(hand.runningTotal < 17)
        {
            // Hit
            hand.Hit();

            if(hand.runningTotal == 21)
            {
                // Blackjack!
                GD.Print("I'm Rich?!");
            }
            if(hand.runningTotal > 21)
            {
                // Bust
                GD.Print("FUCK ME!!!!!");
            }
        } else
        {
            // Stand
            GD.Print("I'm standing!");
        }
    }

    public override void _Process(double delta)
    {

        if(Input.IsActionJustPressed("ui_left")) MakeBJDecision();
        base._Process(delta);
    }

    public void OnBodyEntered(Node3D body)
    {
        if(body is SabotageProp)
        {
            GD.Print("Ow!");
            // (body as SabotageProp).LinearVelocity *= 10.0f;
        }
    }
}
