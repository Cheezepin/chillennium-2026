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
    public float targetOutlineWidth = 0.9f;
    public ShaderMaterial outlineMaterial;

    public override void _Ready()
    {
        front = GetNode<Sprite3D>("Front");
        front.RegionRect = new Rect2(
            (float)rank*(front.RegionRect.Size.X),
            (float)suit*(front.RegionRect.Size.Y),
            front.RegionRect.Size.X,
            front.RegionRect.Size.Y
        );

        Rotation = new Vector3(0, showing ? 0 : Mathf.Pi, 0);
        base._Ready();
    }

    public override void _Process(double delta)
    {

        if(GetParent() is Discard)
        {
            Position = AsymptoticApproach(Position, Vector3.Zero, (float)delta*5.0f);
            GlobalRotation = new Vector3(AsymptoticApproach(GlobalRotation.X, Mathf.Pi*0.5f, 10.0f*(float)delta, 0), 0, 0);
            if(Position.Length() < 0.1f) QueueFree();
        } else
        {
            float targetRot = showing ? 0 : Mathf.Pi;
            Rotation = new Vector3(Rotation.X, AsymptoticApproach(Rotation.Y, targetRot, 10.0f*(float)delta), Rotation.Z);
        }

        if(!useHandColor) {
            front.Modulate = AsymptoticApproach(front.Modulate, targetModulate, 40.0f*(float)delta);
            // outlineMaterial.SetShaderParameter("outlineWidth", 
			//     AsymptoticApproach((float)outlineMaterial.GetShaderParameter("outlineWidth"), targetOutlineWidth, 40.0f*(float)delta));
        }
        base._Process(delta);
    }



    public void OnMouseEnter()
    {
		targetOutlineWidth = 1.2f;
        targetModulate = new Color(0.5f, 0.5f, 0.7f);
    }
	
	public void OnMouseExit()
    {
        targetOutlineWidth = 0.9f;
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
                if(hand.parent.IsAlert())
                {
                    float swapAngryVal = 0;
                    switch(hand.parent.personality)
                    {
                        case Personality.Normal:   swapAngryVal = 0.4f; break;
                        case Personality.Skittish: swapAngryVal = 0.7f; break;
                        case Personality.Lazy:     swapAngryVal = 0.2f; break;
                    }
                    hand.parent.GetAngry(swapAngryVal);
                }
            }
        }
	}
}
