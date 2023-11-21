# Asset Inheritor

Inherits, customizes, and creates collisions for 3D models in godot.
- Recreates recursive folders of 3D models as TSCN godot game objects with collision physics via C#.
- Traverses scene tree depth first editing & restructuring mesh transforms. This helps do things like aligning the mesh origin for a bunch of files at once.

Transforms Specifics
- Moves or Rotates parent meshes while preserving transforms of child meshes.
- Creates collisions of all meshes, places them anywhere you want.
- Changes physics type of root node too.
- Scales root node and root node only.

UX/UI Specifics
- Recalls preset settings upon reopening.
- Made buttons have a different hue for UX clarity.

Last fully tested on Godot 3.3

Main Code is in the Addons/Asset_Inheritor folder.

More specifically the Inherit.cs file.
