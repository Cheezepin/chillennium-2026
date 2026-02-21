using Godot;
using System;
using static Global;

public partial class UI : CanvasLayer
{
    public RichTextLabel moneyCounter;
    public RichTextLabel statusAlert;

    public static UI Instance;

    public static string statusText;
    public static double statusTimer;

    public override void _Ready()
    {
        Instance = this;
        moneyCounter = GetNode<RichTextLabel>("Money");
        statusAlert = GetNode<RichTextLabel>("Status Alert");
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
        base._Process(delta);
    }
}
