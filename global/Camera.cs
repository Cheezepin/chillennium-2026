using Godot;
using System;
using static Global;

public partial class Camera : Camera3D
{
	public static Camera Instance;

	public Vector3 homePos;
	// public Vector3 homeRot;
	// Called when the node enters the scene tree for the first time.
	private float startingRot;
	public override void _Ready()
    {
        Instance = this;
		homePos = Position;
		startingRot = Rotation.X;
		// homeRot = Rotation;
    }

	private double shakeTime = 0;
	private float shakeAmount;

	public float targetRotOffset = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
		Position = homePos;
		// Rotation = homeRot;
        if(shakeTime > 0)
        {
            shakeTime -= delta;
			float angle = rng.RandfRange(0, Mathf.Pi*2.0f);
			// float angle1 = rng.RandfRange(-Mathf.Pi, Mathf.Pi);
			// float angle2 = rng.RandfRange(-Mathf.Pi, Mathf.Pi);
			Position += shakeAmount * new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
			// Rotation += shakeAmount * new Vector3(angle1, angle2, 0);
        }

		Rotation = new Vector3(AsymptoticApproach(Rotation.X, startingRot + targetRotOffset, 10.0f*(float)delta), 0, 0);
    }

	public void ShakeScreen(float amount, double time)
    {
        if(screenShakeEnabled)
        {
            shakeTime = time;
			shakeAmount = amount;
        }
    }
}
