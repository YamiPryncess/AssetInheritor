#if TOOLS
using Godot;
using System;

[Tool]
public class BatchInheritor : EditorPlugin {
    Control dock;
    public override void _EnterTree() {
        //Spatial glb = (Spatial)GD.Load<PackedScene>("res://Assets/KenneyPlatformer/Source/block.glb").Instance();
        //dock = (Control)GD.Load<PackedScene>("addons/my_custom_dock/my_dock.tscn").Instance();
        AddControlToDock(DockSlot.LeftUl, dock);
    }
    
    public override void _ExitTree() {
        RemoveControlFromDocks(dock);
        dock.Free();
    }
}
#endif