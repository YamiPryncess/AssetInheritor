#if TOOLS
using Godot;
using System.Collections.Generic;
[Tool]
public class BII_UI : PanelContainer {
    public VBoxContainer list;
    string importPath;
    string createPath;
    bool recursion;
    string physics;
    string collision;
    bool alterScaling;
    Vector3 scaling;
    bool alterRotation;
    Vector3 rotation;
    bool alterTranslation;
    Vector3 translation;
    public override void _Ready() {
        list = GetNode<VBoxContainer>("Margin/VBox");
    }
    public void _Generate_Pressed() {
        importPath = list.GetNode<LineEdit>("Import/Path").Text;
        createPath = list.GetNode<LineEdit>("Create/Path").Text;
        recursion = list.GetNode<CheckBox>("Recursion").Pressed;
        physics = list.GetNode<OptionButton>("Physics").Text;
        collision = list.GetNode<OptionButton>("Collision").Text;

        alterScaling = list.GetNode<CheckBox>("Scaling/Alter").Pressed;
        scaling = list.GetNode<Vector3Editor>("Scaling").getValue();
        alterRotation = list.GetNode<CheckBox>("Rotation/Alter").Pressed;
        rotation = list.GetNode<Vector3Editor>("Rotation").getValue();
        alterTranslation = list.GetNode<CheckBox>("Translation/Alter").Pressed;
        translation = list.GetNode<Vector3Editor>("Translation").getValue();
        
        File file = new File();
        Directory directory = new Directory();
        if(directory.FileExists(importPath)) {
            Error err1 = file.Open(importPath, File.ModeFlags.Read);
            if(err1 == Error.Ok && GD.Load(importPath) is PackedScene) {
                Node import = GD.Load<PackedScene>(importPath).Instance();
                PackedScene packer = new PackedScene();
                Node remake = remakeScene(import);
                Error err2 = packer.Pack(remake);
                if(err2 == Error.Ok){
                    string name = remake.Name;
                    Error err3 = ResourceSaver.Save(createPath + "/" + name + ".tscn", packer);
                    if(err3 != Error.Ok) GD.Print("Saving at the creation path in did not work.");
                } else {
                    GD.Print("The constructed scene did not work.");
                }
            } else {
                GD.Print("The import file at \"", importPath, "\" did not work.");
            }

        } else if(directory.DirExists(importPath)) {
            
        }
        file.Close();
    }

    public Node remakeScene(Node scene) {
        string rootName = scene.Name;
        Node root;
        List<Node> search = new List<Node>();
        search.Add(scene);
        while (search.Count > 0) {
            Node current = search[0];
            search.RemoveAt(0);
            for(int i = 0; i < current.GetChildCount(); i++) {
                Node child = current.GetChild(i);
                search.Add(child);
                if(child is MeshInstance mesh) {
                    if(alterTranslation) mesh.Translation = translation;
                    if(alterRotation) mesh.Rotation = rotation;
                    if(alterScaling) mesh.Scale = scaling;
                    
                    if(collision != "Default Collision" || collision != "Debug Tangents"){
                        if(collision == "Concave Shape") {
                            mesh.CreateTrimeshCollision();
                        } else if(collision == "Convex Shape") {
                            mesh.CreateConvexCollision();
                        }
                        for(int j = 0; j < mesh.GetChildCount(); j++) {
                            if(mesh.GetChild(j) is StaticBody sb) {
                                for(int k = 0; k < sb.GetChildCount(); k++) {
                                    if(sb.GetChild(k) is CollisionShape cs) {
                                        sb.RemoveChild(cs);
                                        mesh.RemoveChild(sb);
                                        scene.AddChild(cs);
                                        cs.Owner = scene;
                                        sb.QueueFree();
                                    }
                                }
                            }
                        }
                    } else if(collision != "Debug Tangents") {
                        mesh.CreateDebugTangents();
                    }
                }
            }
        }
        root = scene;
        switch(physics) {
            case "Static Body":
                StaticBody staticBody = new StaticBody();
                adopt(scene, staticBody);
                root = staticBody;
                staticBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Rigid Body":
                RigidBody rigidBody = new RigidBody();
                adopt(scene, rigidBody);
                root = rigidBody;
                rigidBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Kinematic Body":
                KinematicBody kinematicBody = new KinematicBody();
                adopt(scene, kinematicBody);
                root = kinematicBody;
                kinematicBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Area Object":
                Area area = new Area();
                adopt(scene, area);
                root = area;
                area.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Bone Object":
                PhysicalBone bone = new PhysicalBone();
                adopt(scene, bone);
                root = bone;
                bone.Name = scene.Name;
                scene.CallDeferred("free");
            break;
        }
        return root;
    }

    public void adopt(Node parent, Node adopter) {
        foreach(Node child in parent.GetChildren()) {
            parent.RemoveChild(child);
            adopter.AddChild(child);
            child.Owner = adopter;
            own(child, adopter);
        }
    }
    public void own(Node parent, Node adopter) {
        foreach(Node child in parent.GetChildren()) {
            child.Owner = adopter;
            own(child, adopter);
        }
    }
}
#endif