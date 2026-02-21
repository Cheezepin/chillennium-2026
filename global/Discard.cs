using Godot;
using System;

public partial class Discard : Node3D
{
    public static Discard Instance;

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }

}
