using Godot;
using System;
using static Global;

public partial class Player : Node3D
{
	// Called when the node enters the scene tree for the first time.

	Vector3 homePos;
	Vector3 targetPos;

	public static Player Instance;

	public SabotageProp rhandProp;
	public Node3D rhandAnchor;
    public Hand frontHandHeld;
    public Node3D frontAnchor;
	public override void _Ready()
    {
        homePos = Position;
		Instance = this;

		rhandAnchor = GetNode<Node3D>("RHandAnchor");
        frontAnchor = GetNode<Node3D>("FrontAnchor");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if(Input.IsActionPressed("ui_accept") && frontHandHeld == null)
        {
            targetPos.Y = homePos.Y - 0.8f;
        } else
        {
            targetPos.Y = homePos.Y;
        }

		Position = new Vector3(Position.X, AsymptoticApproach(Position.Y, targetPos.Y, 10.0f*(float)delta), Position.Z);

        if(frontHandHeld != null && Input.IsActionJustPressed("ui_accept"))
        {
            frontHandHeld.RemoveAnchorToHand();
        }
    }
}
