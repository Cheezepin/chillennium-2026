using Godot;
using System;
using static Global;

public partial class Card : Sprite3D
{

    [Export]public Suit suit;
    [Export]public Rank rank;
    public bool showing = false;

    public override void _Ready()
    {
        RegionRect = new Rect2(
            32.0f + (float)rank*(RegionRect.Size.X),
            32.0f + (float)suit*(RegionRect.Size.Y),
            RegionRect.Size.X,
            RegionRect.Size.Y
        );

        Rotation = new Vector3(0, showing ? 0 : Mathf.Pi, 0);
        base._Ready();
    }

}
