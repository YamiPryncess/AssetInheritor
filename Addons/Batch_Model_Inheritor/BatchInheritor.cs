#if TOOLS
using Godot;
using System;

[Tool]
public class BatchInheritor : EditorPlugin {
    PanelContainer dock;
    public override void _EnterTree() {
        //Spatial glb = (Spatial)GD.Load<PackedScene>("res://Assets/KenneyPlatformer/Source/block.glb").Instance();
        dock = (PanelContainer)GD.Load<PackedScene>("Addons/Batch_Model_Inheritor/Inherit.tscn").Instance();
        AddControlToDock(DockSlot.LeftUr, dock);
        
    }
    
    public override void _ExitTree() {
        RemoveControlFromDocks(dock);
        dock.Free();
    }
}
#endif