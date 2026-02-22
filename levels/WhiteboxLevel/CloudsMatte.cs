using Godot;
using System;

public partial class CloudsMatte : Sprite3D
{
    public float speed = 2.0f;
    public override void _Process(double delta)
    {
        Position += Vector3.Right * speed * (float)delta;
        if(Position.X > 160.0f) Position -= Vector3.Right*320.0f;
        base._Process(delta);
    }
}
