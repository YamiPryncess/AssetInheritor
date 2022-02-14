using Godot;
using System;

public class Preset {
    public string importPath {set; get;}
    public string createPath {set; get;}
    public bool recursion {set; get;}
    public int physics {set; get;}
    public int collision {set; get;}
    public bool alterScaling {set; get;}
    public Vector3 scaling {set; get;}
    public bool alterRotation {set; get;}
    public Vector3 rotation {set; get;}
    public bool alterTranslation {set; get;}
    [Export] public Vector3 translation {set; get;}
}

    // [Export] public Dictionary<string, string> strings1 {set; get;} = new Dictionary<string, string>() {
    //     {"importPath", ""},
    //     {"createPath", ""},
    //     {"physics", ""},
    //     {"collision", ""},
    // }; 
    // [Export] public Dictionary <string, bool> bools1 {set; get;} = new Dictionary<string, bool>() {
    //     {"recursion", true},
    //     {"alterScaling", true},
    //     {"alterRotation", true},
    //     {"alterTranslation", true}
    // };
    // [Export] public Dictionary <string, Vector3> vectors1 {set; get;} = new Dictionary<string, Vector3>() {
    //     {"scaling", new Vector3(1,1,1)},
    //     {"rotation", new Vector3(0,0,0)},
    //     {"translation", new Vector3(0,0,0)}
    // };