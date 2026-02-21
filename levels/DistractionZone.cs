using Godot;
using System;

public partial class DistractionZone : Area3D
{
    [Export]public Node npcs;
    public void OnBodyEntered(Node node)
    {
        if(node is SabotageProp)
        {
            SabotageProp s = node as SabotageProp;
            
            foreach(NPC npc in npcs.GetChildren())
            {
                npc.EmitSignal(NPC.SignalName.GetDistracted, s);
            }
        }
    }
}
