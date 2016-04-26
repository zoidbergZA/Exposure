using UnityEngine;
using System.Collections;

public class UIClass : EditorClass
{
	#region EDIT THESE

	// Edit these to reflect the type of prop you want.
	// These are what your props will be called on the gui.
	public string propOne_Str = "Trees";
	public string propTwo_Str = "Bushes";
	public string propThree_Str = "Rocks";
	public string propFour_Str = "Grass";

	#endregion

	#region Tool Selection

	// For tool selection
	// These shouldn't be changed.
	static string[] toolNames = { "Raise", "Lower", "Flatten" /*, "Props" */};
	string[] propTypes;

	// Main Selection Toolbar
	private string[] mainSelectionToolbarString = new string[] {"Terrain", "Objects"};

	#endregion

	#region private variables

	// These define the position of some gui elements.
	// These are only created for gui elements that share the same position.
	private Rect propsVariation_CountUpButton = new Rect (65, 170, 50, 25);
	private Rect propsVariation_CountDownButton = new Rect (15, 170, 50, 25);
	private Rect propVariation_LabelOne = new Rect (15, 145, 150, 25);
	private Rect propVariation_LabelTwo = new Rect (165, 145, 150, 25);

	// This is thelower and upper limits for the strength of the brush.
	// You should be able to safely edit these if you wish.
	private float pullMin = 1;
	private float pullMax = 20;
	private float radiusMin = 1;
	private float radiusMax = 20;

	// This is the limits determining how high or how low you can make your terrain.
	// You should be able to safely edit these if you wish.
	private float minHeightLimit = 30;
	private float maxHeightLimit = 40;
	private float minDepthLimit = 20;
	private float maxDepthLimit = 30;

	private float minAltitude = 30;
	private float maxAltitude = 40;

	// For mesh saving
	private int meshWindowID; // The ID of the save mesh window.
	private string meshName = "Type mesh name..."; // The name of the saved mesh.

	// Some GUI variables
	// These shouldn't be changed unless you know what you're doing.
/*	private int _xOffset = Screen.width / 2;
	private int _yOffset = Screen.height;
	private int _xSize = 1045; // 870
	private int _ySize = 100;*/

	// Stores the game controller object.
	private GameObject gameController;

	// Only allow mesh saving inside the unity editor.
	// Shouldn't be changed unless you know what you're doing.
	#if UNITY_EDITOR
	private SaveMesh meshSaver;
	#endif

	#endregion

    void Awake ()
    {
		// Gets a unique id for the save mesh window.
        meshWindowID = WindowIDManager.GetID();
    }

    void Start ()
    {
        gameController = GameObject.Find("GameController");

		propTypes = new string[] { propOne_Str, propTwo_Str, propThree_Str, propFour_Str };

		#if UNITY_EDITOR
        meshSaver = gameController.GetComponent<SaveMesh>();
		#endif

	}

	// Even though Update() is empty, you shouldn't remove it otherwise it'll cause trouble.
    void Update() {}

	void OnGUI ()
	{
		DrawPlayButton ();

		if (!PlanetStatistics.playMode)
		{
			#if UNITY_EDITOR
			DrawMeshSaverWindow();
			#endif
			
			GUI.BeginGroup (new Rect (5, 5, 300, 200));
			
			DrawMainBox ();
			DrawToolPropertiesWindow();
			
			// Terrain selected
			if (PlanetStatistics.mainSelection == 0)
			{
				DrawToolsWindow();
			}
			// Objects selected
			if (PlanetStatistics.mainSelection == 1)
			{
				DrawPropTypeSelectionWindow();
				DrawPropVariationSelectionWindow();
				DrawPropSliders ();
			}
			
			GUI.EndGroup ();
		}
	}

	#region drawing methods

	void DrawMainBox ()
	{
		// Main GUI Box
		GUI.Box (new Rect (0, 0, 300, 200), "");
		PlanetStatistics.mainSelection = GUI.Toolbar(new Rect(25, 5, 250, 30), PlanetStatistics.mainSelection, mainSelectionToolbarString);
	}

	void DrawToolsWindow ()
    {
        PlanetStatistics.toolSelection = GUI.SelectionGrid(new Rect(10, 40, 280, 30), PlanetStatistics.toolSelection, toolNames, 3);

		// Raise Terrain
		if (PlanetStatistics.toolSelection == 0)
		{
			DrawLimitsWindow();
		}
		// Lower Terrain
		if (PlanetStatistics.toolSelection == 1)
		{
			DrawLimitsWindow();
		}
		// Flatten Terrain
		if (PlanetStatistics.toolSelection == 2)
		{
			DrawFlattenWindow();
		}
    }

	void DrawPlayButton ()
	{
		if (PlanetStatistics.playMode == true)
		{
			if (GUI.Button (new Rect (Screen.width - 105, 5, 100, 30), "Stop"))
			{
				PlanetStatistics.playMode = false;
			}
		}
		else
		{
			if (GUI.Button (new Rect (Screen.width - 105, 5, 100, 30), "Play"))
			{
				PlanetStatistics.playMode = true;
			}
		}

	}

    void DrawPropTypeSelectionWindow ()
    {
        PlanetStatistics.propSelection = GUI.SelectionGrid(new Rect(10, 40, 280, 30), PlanetStatistics.propSelection, propTypes, 4);
    }

	void DrawPropSliders ()
	{
		// Minimum distance between props
		GUI.Label(new Rect(15, 110, 150, 25), "Min Distance: " + PlanetStatistics.minDistance);
		PlanetStatistics.minDistance = GUI.HorizontalSlider(new Rect(15, 130, 115, 30), Mathf.Round(PlanetStatistics.minDistance), 1.0f, 10.0f);

		// Amount of objects to place
		GUI.Label(new Rect(170, 110, 150, 25), "Amount of Objects: " + PlanetStatistics.amountOfObjects);
		PlanetStatistics.amountOfObjects = GUI.HorizontalSlider(new Rect(170, 130, 115, 30), Mathf.Round(PlanetStatistics.amountOfObjects), 1.0f, 25.0f);
	}

    void DrawPropVariationSelectionWindow ()
    {
        if (PlanetStatistics.propSelection == 0)
        {
            // PropType one
			GUI.Label(new Rect(propVariation_LabelOne), "Change " + propOne_Str + " type:");
			if (GUI.Button(new Rect(propsVariation_CountUpButton), "-->"))
            {
                if (PlanetStatistics.propOneType < oneProps.Count - 1)
                {
					PlanetStatistics.propOneType++;
                }
            }
			if (GUI.Button(new Rect(propsVariation_CountDownButton), "<--"))
            {
				if (PlanetStatistics.propOneType > 0)
                {
					PlanetStatistics.propOneType--;
                }
            }
			GUI.Label(new Rect(propVariation_LabelTwo), oneProps[PlanetStatistics.propOneType].transform.name);
        }

        if (PlanetStatistics.propSelection == 1)
        {
			// PropType two
			GUI.Label(new Rect(propVariation_LabelOne), "Change " + propTwo_Str + " type:");
			if (GUI.Button(new Rect(propsVariation_CountUpButton), "-->"))
            {
                if (PlanetStatistics.propTwoType < twoProps.Count - 1)
                {
					PlanetStatistics.propTwoType++;
                }
            }
			if (GUI.Button(new Rect(propsVariation_CountDownButton), "<--"))
            {
				if (PlanetStatistics.propTwoType > 0)
                {
					PlanetStatistics.propTwoType--;
                }
            }
			GUI.Label(new Rect(propVariation_LabelTwo), twoProps[PlanetStatistics.propTwoType].transform.name);
        }

        if (PlanetStatistics.propSelection == 2)
        {
			// PropType three
			GUI.Label(new Rect(propVariation_LabelOne), "Change " + propThree_Str + " type:");
			if (GUI.Button(new Rect(propsVariation_CountUpButton), "-->"))
            {
                if (PlanetStatistics.propThreeType < threeProps.Count - 1)
                {
					PlanetStatistics.propThreeType++;
                }
            }
			if (GUI.Button(new Rect(propsVariation_CountDownButton), "<--"))
            {
				if (PlanetStatistics.propThreeType > 0)
                {
					PlanetStatistics.propThreeType--;
                }
            }
			GUI.Label(new Rect(propVariation_LabelTwo), threeProps[PlanetStatistics.propThreeType].transform.name);
        }

        if (PlanetStatistics.propSelection == 3)
        {
			// PropType four
			GUI.Label(new Rect(propVariation_LabelOne), "Change " + propFour_Str + " type:");
			if (GUI.Button(new Rect(propsVariation_CountUpButton), "-->"))
            {
                if (PlanetStatistics.propFourType < fourProps.Count - 1)
                {
					PlanetStatistics.propFourType++;
                }
            }
			if (GUI.Button(new Rect(propsVariation_CountDownButton), "<--"))
            {
				if (PlanetStatistics.propFourType > 0)
                {
					PlanetStatistics.propFourType--;
                }
            }
			GUI.Label(new Rect(propVariation_LabelTwo), fourProps[PlanetStatistics.propFourType].transform.name);
        }
    }

	void DrawToolPropertiesWindow ()
	{
		// Brush Strength
		GUI.Label(new Rect(15, 80, 150, 25), "Brush Strength: " + PlanetStatistics.brushStrength);
		PlanetStatistics.brushStrength = GUI.HorizontalSlider(new Rect(15, 100, 115, 30), Mathf.Round(PlanetStatistics.brushStrength), pullMin, pullMax);
		
		// Brush Size
		GUI.Label(new Rect(170, 80, 150, 25), "Brush Size: " + PlanetStatistics.brushSize);
		PlanetStatistics.brushSize = GUI.HorizontalSlider(new Rect(170, 100, 115, 30), Mathf.Round(PlanetStatistics.brushSize), radiusMin, radiusMax);
	}

	void DrawFlattenWindow ()
	{
		// Altitude
		GUI.Label(new Rect(15, 160, 150, 25), "Altitude: " + PlanetStatistics.altitude.ToString("F2"));
		PlanetStatistics.altitude = GUI.HorizontalSlider(new Rect(15, 180, 115, 30), PlanetStatistics.altitude, minAltitude, maxAltitude);
	}
	
	void DrawLimitsWindow ()
	{
		// Height Limit
		GUI.Label(new Rect(15, 160, 150, 25), "Height Limit: " + PlanetStatistics.heightLimit.ToString("F2"));
		PlanetStatistics.heightLimit = GUI.HorizontalSlider(new Rect(15, 180, 115, 30), PlanetStatistics.heightLimit, minHeightLimit, maxHeightLimit);
		
		// Depth Limit
		GUI.Label(new Rect(170, 160, 150, 25), "Depth Limit: " + PlanetStatistics.depthLimit.ToString("F2"));
		PlanetStatistics.depthLimit = GUI.HorizontalSlider(new Rect(170, 180, 115, 30), PlanetStatistics.depthLimit, minDepthLimit, maxDepthLimit);
	}

	#if UNITY_EDITOR
    void DrawMeshSaverWindow ()
    {
        if (meshWindow == true)
        {
            GUI.Window(meshWindowID, new Rect(5, 210, 300, 100), MeshWindow, "Mesh Window");
        }
		else
		{
			GUI.contentColor = Color.black;
			GUI.Label (new Rect (15, 210, 300, 30), "Press the 'm' key to open the Mesh Window");
			GUI.contentColor = Color.white;
		}
    }
    void MeshWindow (int windowID)
    {
        meshName = GUI.TextField(new Rect(5, 25, 150, 25), meshName);

        if (GUI.Button(new Rect(5, 60, 150, 30), "Save Mesh"))
        {
            meshSaver.SaveAsset(meshName);
        }
    }
	#endif

	#endregion
}