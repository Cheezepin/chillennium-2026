using Godot;
using System;
using static Global;

public partial class UI : CanvasLayer
{
    public RichTextLabel moneyCounter;
    public RichTextLabel roundCounter;
    public RichTextLabel statusAlert;

    public ColorRect fadeRect;

    public static UI Instance;

    public static string statusText;
    public static double statusTimer;

    public double fadeTimer = 0;

    [Signal] public delegate void GetHitEventHandler();

    public override void _Ready()
    {
        Instance = this;
        moneyCounter = GetNode<RichTextLabel>("Money");
        roundCounter = GetNode<RichTextLabel>("Round");
        statusAlert = GetNode<RichTextLabel>("Status Alert");
        fadeRect = GetNode<ColorRect>("FadeRect");
        GetHit += GetHitByAttack;
        base._Ready();
    }

    public static void AlertStatus(string text, double timer)
    {
        statusText = text;
        statusTimer = timer;
    }

    public override void _Process(double delta)
    {
        moneyCounter.Text = "$" + money.ToString() + " / " + "$" + quota.ToString();
        roundCounter.Text = "Round " + BlackjackHandler.Instance.round.ToString() + "/3";

        if(statusTimer <= 0)
        {
            statusTimer = 0;
            statusAlert.Hide();
        } else
        {
            statusAlert.Show();
            statusAlert.Text = statusText;
            statusTimer -= delta;
            statusAlert.Modulate = new Color(1,1,1,Mathf.Clamp((float)statusTimer, 0, 1.0f));
        }

        if(fadeTimer > 0)
        {
            fadeTimer -= delta;
            fadeRect.Color = new Color(0,0,0,Mathf.Clamp((float)fadeTimer, 0, 1));
            if(fadeTimer <= 0)
            {
                fadeTimer = 0;
                fadeRect.Hide();
            }
        }
        base._Process(delta);
    }

    public void GetHitByAttack()
    {
        fadeTimer = 2.0;
        fadeRect.Show();
    }
}
