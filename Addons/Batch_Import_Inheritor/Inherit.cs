#if TOOLS
using Godot;
using System.Collections.Generic;
[Tool]
public class Inherit : PanelContainer {
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

        Directory directory = new Directory();
        if(directory.FileExists(importPath)) {
            operateFile(importPath);
        } else if(directory.DirExists(importPath + "/")){
            recurseDir(importPath);
        } else {
            GD.Print("Error: Nothing exists at \"", importPath, "\"");
        }
    }

    public void operateFile(string filePath, string name = "", string nestSave = "") {
        File file = new File();
        string wholeImpPath = name == "" ? filePath : filePath + "/" + name;
        if(wholeImpPath.Substring(wholeImpPath.Length()-7) == ".import") {return;}  
            if(file.Open(wholeImpPath, File.ModeFlags.Read) == Error.Ok && 
                GD.Load(wholeImpPath) is PackedScene) {
                Node import = GD.Load<PackedScene>(wholeImpPath).Instance();
                PackedScene packer = new PackedScene();
                Node remake = remakeScene(import);
                if(packer.Pack(remake) == Error.Ok){
                    name = remake.Name;
                    //GD.Print(createPath + nestSave + "/" + name + ".tscn");
                    Error err = ResourceSaver.Save(createPath + nestSave + "/" + name + ".tscn", packer);
                    if(err != Error.Ok) GD.Print("Error: Saving at the creation path in did not work.");
                } else {
                    GD.Print("Error: The constructed scene did not work.");
                }
            } else {
                //GD.Print("Error: The import file at \"", filePath, "\" did not work.");
                //Gets fired when it runs into anything that isn't a packedscene like a png.
                //Because we can run this recursively I'm going to comment this out.
            }
        file.Close();
    }

    public void recurseDir(string dirPath) {
        Directory dir = new Directory();
        if(dir.Open(dirPath) == Error.Ok) {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while(fileName != ""){
                if(dir.CurrentIsDir() && recursion) {
                    if(fileName != "." && fileName != "..")
                    recurseDir(dir.GetCurrentDir() + "/" + fileName + "/");
                } else if(dir.FileExists(dir.GetCurrentDir() + "/" + fileName) && fileName != "") {
                    //GD.Print(dir.GetCurrentDir() + " & " + fileName);
                    Directory dir2 = new Directory();
                    dir2.Open(importPath);
                    dir2.ListDirBegin();
                    string nestPath = dir.GetCurrentDir().Substring(dir2.GetCurrentDir().Length);
                    //GD.Print(nestPath);
                    dir.MakeDirRecursive(createPath + nestPath + "/");
                    operateFile(dir.GetCurrentDir() + "/", fileName, nestPath);
                    dir2.ListDirEnd();
                }
                fileName = dir.GetNext();
            }
        } else {
            GD.Print("Error: The directory path(s) at \"", dirPath, "\" did not work.");
        }
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
                    
                    if(collision != "Default Collision") {
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