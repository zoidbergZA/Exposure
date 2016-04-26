Spherical Terrain

--------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------

As this is my first package on the asset store, there may be things i have not thought of. 
So if you feel that something is missing or something should be changed, please feel free to contact me at: casper_stougaard@live.dk

--------------------------------------------------------------------------------------------------------------

Adding your own props:
	To be able to place your own props on your sphere, go to:
	Resources/Prefabs/Props/
	And place your props in the appropriate folders.

--------------------------------------------------------------------------------------------------------------

Editing the names of your props:
	Open UIClass.cs
	At the beginning of the script you will see a region named "EDIT THESE"
	Insde you will find the appropriate prop string names
	
--------------------------------------------------------------------------------------------------------------

Saving your mesh:
	To save your mesh, hit the "m" key, and a window should pop up.
	Type in your desired name, and hit the "Save Mesh" button.

--------------------------------------------------------------------------------------------------------------

Using your saved mesh:
	To use your saved mesh, go to:
	Resources/Meshes/SavedMeshes/"The name of your mesh"/
	Now drag your mesh onto the "Mesh Filter" of your object.
	
--------------------------------------------------------------------------------------------------------------

Changing textures:
1A:	In your project view, go to Resources/Art/Materials
	Click "PlanetMaterial"
	
	OR
	
1B:	In your Hierarchy view, click the "Globe" gameobject
	Extend the "PlanetMaterial"
	
2:	In your inspector view you will see the shader settings
	Textures are lined up with the deepest altitude at the top and the highest altitude at the bottom
	
3:	At the bottom you will see the blend settings
	You can play around with these as you please
	
	Here are the standard blend settings in case something happens :)
	28,     29,   31,     32, 
	32.5,   33,   33.5,   34.5, 
	35,     36,   36.5,   37
	
--------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------