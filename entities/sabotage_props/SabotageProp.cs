using Godot;
using System;
using static Global;

public partial class SabotageProp : RigidBody3D
{
	// Called when the node enters the scene tree for the first time.
	CsgSphere3D mesh;
	ShaderMaterial outlineMaterial;
	float targetOutlineWidth = 0.9f;

	public override void _Ready()
    {
		mesh = GetNode<CsgSphere3D>("CSGSphere3D");
        outlineMaterial = (ShaderMaterial)mesh.Material.NextPass;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        outlineMaterial.SetShaderParameter("outlineWidth", 
			AsymptoticApproach((float)outlineMaterial.GetShaderParameter("outlineWidth"), targetOutlineWidth, 40.0f*(float)delta));

		if(Player.Instance.rhandProp == this)
        {
            GlobalPosition = Player.Instance.rhandAnchor.GlobalPosition;

			if(Input.IsMouseButtonPressed(MouseButton.Right))
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
				}
			}
        }
	}
}
