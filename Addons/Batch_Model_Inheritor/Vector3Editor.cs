#if TOOLS
using Godot;
using System.Linq;
[Tool]
public class Vector3Editor : VBoxContainer {
    public SpinBox[] spinBoxes = new SpinBox[3];
    public override void _Ready() {
        int i = 0;
        foreach(SpinBox child in GetNode("HBox").GetChildren().OfType<SpinBox>()){
            spinBoxes[i] = child;
            i++;
        }
    }
    public Vector3 getValue() {
        Vector3 values = new Vector3();
        for(int i = 0; i < spinBoxes.Length; i++) {
            values[i] = (float)spinBoxes[i].Value;
        }
        return values;
    }

    public void setValue(Vector3 value) {
        for(int i = 0; i < spinBoxes.Length; i++) {
            spinBoxes[i].Value = value[i];
        }
    }
}
#endif