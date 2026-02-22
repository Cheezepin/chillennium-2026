using Godot;
using System;
using static Global;
using static UI;

public partial class Otter : CharacterBody3D
{
	public static Otter Instance;

	public static NPC target;

	public double walkTimer = 0;
	public float walkSpeed = 0.4f;

	[Export]public AnimationPlayer animPlayer;

    public override void _Ready()
    {
		Instance = this;
        base._Ready();
    }

	public override void _Process(double delta)
	{
		// if(target != null && walkTimer == 0)
        // {
		// 	walkTimer = 0;
        //     // attack
        // } else
        // {
			float radius = 3.5f;
			float zOffset = -0.9f;
            GlobalPosition = radius*new Vector3(Mathf.Sin(walkSpeed*(float)walkTimer), 0, Mathf.Cos(walkSpeed*(float)walkTimer)) + new Vector3(0, 0, zOffset);
			Rotation = new Vector3(0, walkSpeed*(float)walkTimer - Mathf.Pi*0.5f, 0);  
			walkTimer += delta;
			animPlayer.Play("Otter Anims/OtterWalk");
			// if(target != null)
            // {
                // if(GlobalPosition.Z > 0.5f) walkTimer = 0;
            // }
        // }
	}

	public void OnBodyEntered(Node3D body)
    {
        if(body is SabotageProp)
        {
            SabotageProp s = body as SabotageProp;
            if(!s.stillEffective) return;
            Camera.Instance.ShakeScreen(0.01f, 0.07);
            (body as SabotageProp).LinearVelocity *= 4.0f;
            s.stillEffective = false;
            s.GetNode<AudioStreamPlayer>("Hit").Play();
			UI.AlertStatus("-$200 for assaulting waitstaff...", 3.0);
			money -= 200;
        }
    }
}
