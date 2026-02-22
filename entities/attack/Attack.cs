using Godot;
using System;
using static Global;
using static UI;

public partial class Attack : RigidBody3D
{
    public NPC spawner;

    public double despawnTimer = 0;

    public override void _PhysicsProcess(double delta)
    {
        if(Position.Y < 0) QueueFree();

        if(despawnTimer > 0)
        {
            despawnTimer -= delta;
            if(despawnTimer <= 0) QueueFree();
        }
        base._Process(delta);
    }


    public void Despawn()
    {
        GetNode<AudioStreamPlayer>("Hit").Play();
        despawnTimer = 0.5;
        money -= 125;
        BlackjackHandler.Instance.freezeTimer = 3.5;
        AlertStatus(spawner.Name + " stole $" + (125).ToString() + "...", 3.5);
        // QueueFree();
    }
}
