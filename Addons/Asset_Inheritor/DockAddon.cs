#if TOOLS
using Godot;
using System;

[Tool]
public class DockAddon : EditorPlugin {
    PanelContainer dock;
    public override void _EnterTree() {
        dock = (PanelContainer)GD.Load<PackedScene>("Addons/Asset_Inheritor/Inherit.tscn").Instance();
        AddControlToDock(DockSlot.LeftUr, dock);
        
    }
    
    public override void _ExitTree() {
        RemoveControlFromDocks(dock);
        dock.Free();
    }
}
#endif