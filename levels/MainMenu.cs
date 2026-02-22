using Godot;
using System;
using static Global;

public partial class MainMenu : Control
{
	[Export]public Control HowTo;
	[Export]public Control Credits;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ClickStart()
    {
        GetTree().ChangeSceneToFile("levels/WhiteboxLevel/WhiteboxLevel.tscn");
		money = 5000;
		quota = 5500;
    }

	public void ClickHowTo()
    {
        HowTo.Show();
    }

	public void ClickCredits()
    {
        Credits.Show();
    }

	public void Quit()
    {
        GetTree().Quit();
    }

	public void QuitSubmenu()
    {
        Credits.Hide();
		HowTo.Hide();
    }
}
