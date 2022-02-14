#if TOOLS
using Godot;
using System;
[Tool]
public class GenerateButton : Button {
    float totalTime = 0;
    bool setOff = false;
    // string highlighted = "#ae00ff";
    // string normal = "#7a00b3";

    public override void _Ready() {
        
    }
    public override void _Process(float delta) {
        if(HasFocus() && setOff) {
            totalTime += delta;
            if(totalTime >= .0575) {
                ReleaseFocus();
                totalTime = 0;
                setOff = false;
            }
        }
    }
    public void _Button_Down() {

    }
    public void _Button_Up() {
        setOff = true;
    }

}
#endif

    // GD.Print("Button Down");
    // StyleBoxFlat styleBox = new StyleBoxFlat();
    // styleBox.BgColor = new Godot.Color(highlighted);
    // //AddStyleboxOverride("Normal", styleBox);