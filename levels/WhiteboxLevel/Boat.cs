using Godot;
using System;

public partial class Boat : Sprite3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        Position -= Vector3.Right*(float)delta*20.0f;
		if(Position.X < -55.0f) Position += Vector3.Right*110.0f;
    }
}
