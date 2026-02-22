using Godot;
using System;
using static Global;

public partial class SabotageProp : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public MeshInstance3D mesh;
	
	public CollisionShape3D col;
	ShaderMaterial outlineMaterial;
	float targetOutlineWidth = 0.9f;

	public bool selectable = true;
	public bool stillEffective = true;
	
	[Export]public double distractionBaseTime = 1.0;

	public enum PropType
    {
        Throwable,
		Card,
		Radio,
    }

	[Export]public PropType propType;

	[Export]public Suit suit;
	[Export]public Rank rank;

	private Sprite3D front;

	public override void _Ready()
    {
		mesh = GetNodeOrNull<MeshInstance3D>("Mesh");
		col = GetNode<CollisionShape3D>("CollisionShape3D");
        if(mesh != null) outlineMaterial = (ShaderMaterial)mesh.Mesh.SurfaceGetMaterial(0).NextPass;
		selectable = true;

		if(propType == PropType.Card)
        {
            front = GetNode<Sprite3D>("Front");
			front.RegionRect = new Rect2(
				(float)rank*(front.RegionRect.Size.X),
				(float)suit*(front.RegionRect.Size.Y),
				front.RegionRect.Size.X,
				front.RegionRect.Size.Y
			);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if(!selectable) targetOutlineWidth = 0.9f;
        if(outlineMaterial != null) outlineMaterial.SetShaderParameter("outlineWidth", 
			AsymptoticApproach((float)outlineMaterial.GetShaderParameter("outlineWidth"), targetOutlineWidth, 40.0f*(float)delta));
		if(propType == PropType.Card)
        {
            Color targetModulate = targetOutlineWidth > 1.0f ? new Color(0.5f, 0.5f, 0.7f) : new Color(1.0f, 1.0f, 1.0f);
			front.Modulate = AsymptoticApproach(front.Modulate, targetModulate, 40.0f*(float)delta);
        }

		if(Player.Instance.rhandProp == this)
        {
            GlobalPosition = Player.Instance.rhandAnchor.GlobalPosition;

			if(mouseClicked)
            {
                Vector3 camDir = Camera.Instance.ProjectRayNormal(GetViewport().GetMousePosition());
				Vector3 dir = camDir.Normalized();
				GlobalPosition = Camera.Instance.GlobalPosition;
				Freeze = false;
				ApplyCentralImpulse(dir*20.0f);
				Player.Instance.rhandProp = null;
            }
        }
    }

	public void OnMouseEnter()
    {
		if(Player.Instance.rhandProp == this) return;
		targetOutlineWidth = 1.2f;
    }
	
	public void OnMouseExit()
    {
		if(Player.Instance.rhandProp == this) return;
        targetOutlineWidth = 0.9f;
    }

	public void AnchorToHand()
    {
        Player.Instance.rhandProp = this;
		targetOutlineWidth = 0.9f;
    }

	public void OnInputEvent(Node cam, InputEvent inputEvent, Vector3 eventPosition, Vector3 normal, int shapeIdx) {
		if(inputEvent is InputEventMouseButton)
        {
			if(Player.Instance.rhandProp == this)
            {
                
            } else {
				if(((inputEvent as InputEventMouseButton).ButtonMask & MouseButtonMask.Left) != 0)
				{
					switch(propType) {
						case PropType.Throwable:
							AnchorToHand();
							break;
						case PropType.Card:
							Card card = cardScene.Instantiate<Card>();
							card.suit = suit;
							card.rank = rank;
							card.showing = true;
							hands[0].AddChild(card);
							card.GlobalPosition = Deck.Instance.GlobalPosition;

							foreach(NPC npc in BlackjackHandler.Instance.npcs)
                            {
                                if(npc.IsAlert())
                                {
                                    npc.GetAngry(0.4f);
                                }
                            }

							BlackjackHandler.Instance.InterruptBJ();

							QueueFree();
							break;
						case PropType.Radio:
							break;
					}
					mouseClicked = false;
					mouseHeld = true;
					selectable = false;
				}
			}
        }
	}
}
