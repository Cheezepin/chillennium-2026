using Godot;
using System;
using static Global;
using static UI;

public partial class Attack : RigidBody3D
{
    public NPC spawner;

    public override void _PhysicsProcess(double delta)
    {
        if(Position.Y < 0) QueueFree();
        base._Process(delta);
    }


    public void Despawn()
    {
        money -= spawner.bet * 3;
        BlackjackHandler.Instance.freezeTimer = 3.5;
        AlertStatus(spawner.Name + " stole $" + (spawner.bet*3).ToString() + "...", 3.5);
        QueueFree();
    }
}
