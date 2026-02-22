using Godot;
using System;
using static Global;

public partial class SeagullSound : AudioStreamPlayer3D
{
    double timer = 0;
    public void _Process(double delta)
    {
        if(!Playing && timer <= 0)
        {
            Play();
            timer = rng.RandfRange(30.0f, 48.0f);
        }
        timer -= delta;
    }
}
