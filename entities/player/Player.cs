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
            targetPos.Y = homePos.Y - 1.7f;
            Camera.Instance.targetRotOffset = Mathf.Pi * 0.15f;
        } else
        {
            targetPos.Y = homePos.Y;
            Camera.Instance.targetRotOffset = 0;
        }

		Position = new Vector3(Position.X, AsymptoticApproach(Position.Y, targetPos.Y, 10.0f*(float)delta), Position.Z);

        if(frontHandHeld != null && Input.IsActionJustPressed("ui_accept"))
        {
            frontHandHeld.RemoveAnchorToHand();
        }

        // if(Input.IsActionJustPressed("ui_left")) hands[1].parent.GetAngry(1.0f);
        // if(Input.IsActionJustPressed("ui_up")) hands[2].parent.GetAngry(1.0f);
        // if(Input.IsActionJustPressed("ui_right")) hands[3].parent.GetAngry(1.0f);
    }

    public void OnBodyEntered(Node3D body)
    {
        if(body is Attack)
        {
            (body as Attack).Despawn();
            UI.Instance.EmitSignal(UI.SignalName.GetHit);
        }
    }
}
