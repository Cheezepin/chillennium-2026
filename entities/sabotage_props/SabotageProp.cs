using Godot;
using System;
using static Global;

public partial class SabotageProp : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	public CsgSphere3D mesh;
	public CollisionShape3D col;
	ShaderMaterial outlineMaterial;
	float targetOutlineWidth = 0.9f;

	public bool selectable = true;
	
	public double distractionBaseTime = 1.0;

	public override void _Ready()
    {
		mesh = GetNode<CsgSphere3D>("CSGSphere3D");
		col = GetNode<CollisionShape3D>("CollisionShape3D");
        outlineMaterial = (ShaderMaterial)mesh.Material.NextPass;
		selectable = true;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		if(!selectable) targetOutlineWidth = 0.9f;
        outlineMaterial.SetShaderParameter("outlineWidth", 
			AsymptoticApproach((float)outlineMaterial.GetShaderParameter("outlineWidth"), targetOutlineWidth, 40.0f*(float)delta));

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
					AnchorToHand();
					mouseClicked = false;
					mouseHeld = true;
					selectable = false;
				}
			}
        }
	}
}
