using UnityEngine;
using System.Collections;

public static class PlanetStatistics
{
    // Editing
	public static int mainSelection { get; set; }
    public static int toolSelection { get; set; }
    public static int propSelection { get; set; }
    public static int meshSelection { get; set; }

	// Prop placement
    public static int propOneType { get; set; }
	public static int propTwoType { get; set; }
	public static int propThreeType { get; set; }
	public static int propFourType { get; set; }

	public static float propDensity { get; set; }
	public static float minDistance { get; set; }
	public static float amountOfObjects { get; set; }
	
    // Statistics
    public static float waterLevel { get; set; }
    public static int populationCount { get; set; }

	// For UI
	public static float brushSize { get; set; }
	public static float brushStrength { get; set; }

	// Deformation Tools
	public static float heightLimit { get; set; }
	public static float depthLimit { get; set; }

	// Flatten Tool
	public static float altitude { get; set; }





	public static bool playMode { get; set; }
}
