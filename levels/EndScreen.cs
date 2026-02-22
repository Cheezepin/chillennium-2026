using Godot;
using System;
using static Global;

public partial class EndScreen : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<RichTextLabel>("Text").Text = "You left today with $" + money.ToString() + " out of your $" + quota.ToString() 
			+ " quota ...\n\n" + (money >= quota ? "Congrats!" : "Too bad!");
	}

	public void QuitToTitle()
    {
        GetTree().ChangeSceneToFile("levels/TitleScreen.tscn");
    }
}
