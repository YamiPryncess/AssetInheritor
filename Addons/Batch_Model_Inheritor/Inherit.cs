#if TOOLS
using Godot;
using Newtonsoft.Json;
[Tool]
public class Inherit : PanelContainer {
    Node root = null;
    Node import = null;
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
    Preset preset = new Preset();
    string presetPath = "res://Addons/Batch_Model_Inheritor/Preset.json";
    public override void _Ready() {
        File file = new File();
        if(file.FileExists(presetPath) && file.Open(presetPath, File.ModeFlags.Read) == Error.Ok) {
            preset = JsonConvert.DeserializeObject<Preset>(file.GetAsText());
            if(preset != null) {
                list = GetNode<VBoxContainer>("Margin/VBox");

                list.GetNode<LineEdit>("Import/Path").Text = preset.importPath;
                list.GetNode<LineEdit>("Create/Path").Text = preset.createPath;
                list.GetNode<CheckBox>("HBox/Recursion").Pressed = preset.recursion;
                list.GetNode<OptionButton>("Physics").Selected = preset.physics;
                list.GetNode<OptionButton>("Collision").Selected = preset.collision;

                list.GetNode<CheckBox>("Scaling/Alter").Pressed = preset.alterScaling;
                list.GetNode<Vector3Editor>("Scaling").setValue(preset.scaling);
                list.GetNode<CheckBox>("Rotation/Alter").Pressed = preset.alterRotation;
                list.GetNode<Vector3Editor>("Rotation").setValue(preset.rotation);
                list.GetNode<CheckBox>("Translation/Alter").Pressed = preset.alterTranslation;
                list.GetNode<Vector3Editor>("Translation").setValue(preset.translation);
            }
        }
        file.Close();
    }
    public void gatherNodes() {
        list = GetNode<VBoxContainer>("Margin/VBox");

        importPath = list.GetNode<LineEdit>("Import/Path").Text;
        createPath = list.GetNode<LineEdit>("Create/Path").Text;
        recursion = list.GetNode<CheckBox>("HBox/Recursion").Pressed;
        physics = list.GetNode<OptionButton>("Physics").Text;
        collision = list.GetNode<OptionButton>("Collision").Text;

        alterScaling = list.GetNode<CheckBox>("Scaling/Alter").Pressed;
        scaling = list.GetNode<Vector3Editor>("Scaling").getValue();
        alterRotation = list.GetNode<CheckBox>("Rotation/Alter").Pressed;
        rotation = list.GetNode<Vector3Editor>("Rotation").getValue();
        alterTranslation = list.GetNode<CheckBox>("Translation/Alter").Pressed;
        translation = list.GetNode<Vector3Editor>("Translation").getValue();
    }
    public void _Preset_Pressed() {
        gatherNodes();
        Preset preset = new Preset();
        preset.importPath = importPath;
        preset.createPath = createPath;
        preset.recursion = recursion;
        preset.physics = list.GetNode<OptionButton>("Physics").Selected;
        preset.collision = list.GetNode<OptionButton>("Collision").Selected;
        preset.alterScaling = alterScaling;
        preset.scaling = scaling;
        preset.alterRotation = alterRotation;
        preset.rotation = rotation;
        preset.alterTranslation = alterTranslation;
        preset.translation = translation;

        string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
        File file = new File();
        file.Open(presetPath, File.ModeFlags.Write);
        file.StoreString(json);
        file.Close();
    }
    public void _Generate_Pressed() {
        gatherNodes();
        root = null;
        import = null;
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
                import = GD.Load<PackedScene>(wholeImpPath).Instance();
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
                //Because we want to run this recursively through files I'm going to comment this out.
            }
        root = null;
        import = null;
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

    public Node remakeScene(Node scene, bool transform = true, bool dip = false) {
        bool dipTransform = transform;
        for(int i = 0; i < scene.GetChildCount(); i++) {
            Node child = scene.GetChild(i);
            if(child is MeshInstance mesh) {
                if(transform) {//Trans & rot are only applied to parent meshes, not children meshes.
                    if(alterTranslation) mesh.Translation = translation;
                    if(alterRotation) mesh.Rotation = rotation;
                    dipTransform = false;
                }//Scale is only applied to the root node. Idk if you can import scaled children meshes.
                //mesh.Scale = new Vector3(1,1,1);//Uncomment this to descale nested meshes if it's a problem. 
                if(collision != "Native Collision") {
                    if(collision == "Concave Sibling" || collision == "Concave Child") {
                        mesh.CreateTrimeshCollision();
                    } else if(collision == "Convex Sibling" || collision == "Convex Child") {
                        mesh.CreateConvexCollision();
                    }
                    if(collision == "Concave Sibling" || collision == "Convex Sibling") {
                        for(int j = 0; j < mesh.GetChildCount(); j++) {
                            if(mesh.GetChild(j) is StaticBody sb) {
                                for(int k = 0; k < sb.GetChildCount(); k++) {
                                    if(sb.GetChild(k) is CollisionShape cs) {
                                        sb.RemoveChild(cs);
                                        mesh.RemoveChild(sb);
                                        import.AddChild(cs);
                                        cs.Owner = import;
                                        sb.QueueFree();
                                        cs.Translation = mesh.Translation; //Parent or Child Meshes position
                                        cs.Rotation = rotation; //User ui input
                                        cs.Scale = mesh.Scale; //Incase it's possible to import scaled
                                        break;//children meshes. We share the scale w/ cs for consistency.
                                    }
                                } break;
                            }
                        }
                    } else if(collision == "Concave Child" || collision == "Convex Child") {
                        for(int j = 0; j < mesh.GetChildCount(); j++) {
                            if(mesh.GetChild(j) is StaticBody sb) {
                                for(int k = 0; k < sb.GetChildCount(); k++) {
                                    if(sb.GetChild(k) is CollisionShape cs) {
                                        sb.RemoveChild(cs);
                                        mesh.RemoveChild(sb);
                                        Node phyNode = choosePhysics(sb, false);
                                        phyNode.AddChild(cs);
                                        mesh.AddChild(phyNode);
                                        phyNode.Owner = import;
                                        cs.Owner = import;

                                        if(physics == "Rigid Body" &&  
                                        collision == "Concave Child") {
                                            RigidBody rigidBody = (RigidBody)phyNode;
                                            rigidBody.Mode = RigidBody.ModeEnum.Static;
                                        }
                                        break;
                                    }
                                } break;
                            }
                        }
                    }
                }
            }
            remakeScene(child, dipTransform, true);
        }//Duplicating your model in your model editor and I believe naming it "$name-col" (search the docs)
        //makes it so godot implements a collision in place of the duplicate within the native import.
        //Godot also can also change the root node of the import for you too.
        //In order to preserve such functionality we have Native Physics and Native Collision options.
        //The native import root node and duplicate model renamed to -col for collision can be kept intact.
        //This plugin still provides more options than godot & is a lot more seamless than 
        //duplicating and renaming in blender. This plugin can also change the transform of parent meshes,
        //set collisions as children or siblings, and set physics as root or children.
        //It doesn't delete or change nested nodes on the other hand because they may be needed for the
        //import. For example an animation player may be connected to multiple nodes in the scene.
        if(dip == false) {
            if(physics != "Native Physics" && (collision == "Native Collision" ||
                collision == "Concave Sibling" || collision == "Convex Sibling")) {
                    root = choosePhysics(import); 
                    if(physics == "Rigid Body" && collision == "Concave Sibling") {
                            RigidBody rigidBody = (RigidBody)root;
                            rigidBody.Mode = RigidBody.ModeEnum.Static;
                    } //Here we have the rigid body with concave interaction. It rigid has to be static.
            }
        }
        root = root != null ? root : import;
        if(root is Spatial spatial) {
            spatial.Scale = scaling;
        }//The only place we change scaling because scaled children translate different.
        return root;
    }

    public Node choosePhysics(Node scene, bool rebuild = true) {
        Node phyNode = new Node();
        switch(physics) {
            case "Native Physics": //This one just gives child collisions their static body back.
                phyNode = scene;
            break;
            case "Static Body":
                StaticBody staticBody = new StaticBody();
                if(rebuild) adopt(scene, staticBody);
                phyNode = staticBody;
                staticBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Rigid Body":
                RigidBody rigidBody = new RigidBody();
                if(rebuild) adopt(scene, rigidBody);
                phyNode = rigidBody;
                rigidBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Kinematic Body":
                KinematicBody kinematicBody = new KinematicBody();
                if(rebuild) adopt(scene, kinematicBody);
                phyNode = kinematicBody;
                kinematicBody.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Area Object":
                Area area = new Area();
                if(rebuild) adopt(scene, area);
                phyNode = area;
                area.Name = scene.Name;
                scene.CallDeferred("free");
            break;
            case "Bone Object":
                PhysicalBone bone = new PhysicalBone();
                if(rebuild) adopt(scene, bone);
                phyNode = bone;
                bone.Name = scene.Name;
                scene.CallDeferred("free");
            break;
        }
        return phyNode;
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