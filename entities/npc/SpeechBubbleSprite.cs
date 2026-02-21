using Godot;
using System;

public partial class SpeechBubbleSprite : Sprite3D
{
    public double dialogTimer = 0;

    public override void _Process(double delta)
    {
        dialogTimer += delta;

        float scaleVal = EaseOutBack(Mathf.Clamp((float)dialogTimer, 0.0f, 1.0f));
        scaleVal *= 0.15f;
        Scale = new Vector3(scaleVal, scaleVal, scaleVal);

        Modulate = new Color(1,1,1,Mathf.Clamp(5.0f - (float)dialogTimer, 0.0f, 1.0f));
        base._Process(delta);
    }

    float EaseOutBack(float x) {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }

}
