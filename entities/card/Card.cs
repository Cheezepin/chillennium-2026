using Godot;
using System;
using static Global;

public partial class Card : Area3D
{

    [Export]public Suit suit;
    [Export]public Rank rank;

    public Sprite3D front;
    public bool showing = false;

    public bool useHandColor;
    public Color targetModulate = new Color(1,1,1);

    public override void _Ready()
    {
        front = GetNode<Sprite3D>("Front");
        front.RegionRect = new Rect2(
            32.0f + (float)rank*(front.RegionRect.Size.X),
            32.0f + (float)suit*(front.RegionRect.Size.Y),
            front.RegionRect.Size.X,
            front.RegionRect.Size.Y
        );

        Rotation = new Vector3(0, showing ? 0 : Mathf.Pi, 0);
        base._Ready();
    }

    public override void _Process(double delta)
    {
        float targetRot = showing ? 0 : Mathf.Pi;
        Rotation = new Vector3(0, AsymptoticApproach(Rotation.Y, targetRot, 20.0f*(float)delta), 0);

        if(GetParent() is Discard)
        {
            Position = AsymptoticApproach(Position, Vector3.Zero, (float)delta*5.0f);
            if(Position.Length() < 0.1f) QueueFree();
        }

        if(!useHandColor) front.Modulate = targetModulate;
        base._Process(delta);
    }



    public void OnMouseEnter()
    {
		// targetOutlineWidth = 1.2f;
        targetModulate = new Color(1.0f, 1.0f, 0.0f);
    }
	
	public void OnMouseExit()
    {
        // targetOutlineWidth = 0.9f;
        targetModulate = new Color(1.0f, 1.0f, 1.0f);
    }

	public void OnInputEvent(Node cam, InputEvent inputEvent, Vector3 eventPosition, Vector3 normal, int shapeIdx) {
		if(inputEvent is InputEventMouseButton)
        {
            if(((inputEvent as InputEventMouseButton).ButtonMask & MouseButtonMask.Left) != 0)
            {
                if(useHandColor) return;
                Hand hand = GetParent<Hand>();
                hand.SwapCard(this);
            }
        }
	}
}
