using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EditorClass : MonoBehaviour
{
	public GameObject playerSpawn;
	protected static Transform editorTransform;
	UIClass uiClass;
	
	public enum FallOff { Gauss, Linear, Needle };
	public static FallOff fallOff = FallOff.Gauss;

	// MESH
	static Mesh mesh;
	static MeshCollider meshCollider;
	public static Vector3[] points;
	static Vector3[] vertices;
	static int[] triangles;
	static Vector2[] uvs;

	//	Properties
    protected static List<GameObject> oneProps = new List<GameObject>();
	protected static List<GameObject> twoProps = new List<GameObject>();
    protected static List<GameObject> threeProps = new List<GameObject>();
    protected static List<GameObject> fourProps = new List<GameObject>();

	private RaycastHit hit;
	private Ray ray;

    public static bool meshWindow = false;
	protected bool playerActive = false;

	#region planned for future updates

	// Everything in here is preperation for future updates
	protected float waterLevel = 62;
	
	#endregion

	void Start ()
	{
		uiClass = GameObject.Find ("GameController").GetComponent<UIClass> ();
		transform.FindChild ("Projector").gameObject.SetActive (true);
		mesh = GetComponent<MeshFilter>().mesh;
		Spheriphy();

		oneProps = Resources.LoadAll("Prefabs/Props/" + uiClass.propOne_Str, typeof(GameObject)).Cast<GameObject>().ToList();
		twoProps = Resources.LoadAll("Prefabs/Props/" + uiClass.propTwo_Str, typeof(GameObject)).Cast<GameObject>().ToList();
        threeProps = Resources.LoadAll("Prefabs/Props/" + uiClass.propThree_Str, typeof(GameObject)).Cast<GameObject>().ToList();
        fourProps = Resources.LoadAll("Prefabs/Props/" + uiClass.propFour_Str, typeof(GameObject)).Cast<GameObject>().ToList();

		playerSpawn = GameObject.Find ("PlayerSpawn");
		playerSpawn.SetActive (false);
	}

	void Update ()
	{
        if (waterLevel != PlanetStatistics.waterLevel)
        {
            PlanetStatistics.waterLevel = waterLevel;
        }

        if (Input.GetKeyUp(KeyCode.M))
        {
            meshWindow = !meshWindow;
        }

		TogglePlayMode ();
	}

	void TogglePlayMode ()
	{
		//Debug.Log (PlanetStatistics.playMode);
		if (PlanetStatistics.playMode)
		{
			if (!playerActive)
			{
				playerSpawn.SetActive(true);
				playerActive = true;
			}
		}
		if (!PlanetStatistics.playMode)
		{
			if (playerActive)
			{
				playerSpawn.SetActive(false);
				playerActive = false;
			}
			ToolSelection();
		}
	}

    void ToolSelection ()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0) && PlanetStatistics.toolSelection != 3)
        {
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				// Raise Terrain
				if (PlanetStatistics.mainSelection == 0 && PlanetStatistics.toolSelection == 0)
				{
					MeshFilter filter = hit.collider.GetComponent<MeshFilter>();
					if (filter)
					{
						if (filter != ToolsClass.unappliedMesh)
						{
							ToolsClass.unappliedMesh = filter;
						}
						
						// Deform mesh
						var relativePoint = filter.transform.InverseTransformPoint(hit.point);
						ToolsClass.DeformMesh(filter.mesh, relativePoint, PlanetStatistics.brushStrength * Time.deltaTime, PlanetStatistics.brushSize);
					}	
				}
				// Lower Terrain
				if (PlanetStatistics.mainSelection == 0 && PlanetStatistics.toolSelection == 1)
				{
					MeshFilter filter = hit.collider.GetComponent<MeshFilter>();
					if (filter)
					{
						if (filter != ToolsClass.unappliedMesh)
						{
							ToolsClass.unappliedMesh = filter;
						}
						
						// Deform mesh
						var relativePoint = filter.transform.InverseTransformPoint(hit.point);
						ToolsClass.DeformMesh(filter.mesh, relativePoint, -PlanetStatistics.brushStrength * Time.deltaTime, PlanetStatistics.brushSize);
					}
				}
				// Flatten Terrain.
				if (PlanetStatistics.mainSelection == 0 && PlanetStatistics.toolSelection == 2)
				{
					MeshFilter filter = hit.collider.GetComponent<MeshFilter>();
					if (filter)
					{
						if (filter != ToolsClass.unappliedMesh)
						{
							ToolsClass.unappliedMesh = filter;
						}
						var relativePoint = filter.transform.InverseTransformPoint(hit.point);
						ToolsClass.FlattenMesh(hit.transform, filter.mesh, relativePoint, PlanetStatistics.brushStrength * Time.deltaTime, PlanetStatistics.altitude, PlanetStatistics.brushSize);
					}
				}
			} // Raycast
        } // Input mouse(0)

        if (Input.GetMouseButtonUp(0))
        {
            ApplyMeshCollider();

			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				MeshFilter filter = hit.collider.GetComponent<MeshFilter>();
				if (PlanetStatistics.mainSelection == 1)
				{
					if (PlanetStatistics.propSelection == 0)
					{
						ToolsClass.PlaceGroupProp(filter.mesh, gameObject, oneProps[PlanetStatistics.propOneType] as GameObject, hit.point, PlanetStatistics.brushSize, PlanetStatistics.amountOfObjects);
					}
					if (PlanetStatistics.propSelection == 1)
					{
						ToolsClass.PlaceGroupProp(filter.mesh, gameObject, twoProps[PlanetStatistics.propTwoType] as GameObject, hit.point, PlanetStatistics.brushSize, PlanetStatistics.amountOfObjects);
					}
					if (PlanetStatistics.propSelection == 2)
					{
						ToolsClass.PlaceGroupProp(filter.mesh, gameObject, threeProps[PlanetStatistics.propThreeType] as GameObject, hit.point, PlanetStatistics.brushSize, PlanetStatistics.amountOfObjects);
					}
					if (PlanetStatistics.propSelection == 3)
					{
						ToolsClass.PlaceGroupProp(filter.mesh, gameObject, fourProps[PlanetStatistics.propFourType] as GameObject, hit.point, PlanetStatistics.brushSize, PlanetStatistics.amountOfObjects);
					}
				}
        	}
		}
    }

	void Spheriphy ()
	{
		vertices = mesh.vertices;
		triangles = mesh.triangles;
		uvs = mesh.uv;
		
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.Optimize();
		mesh.RecalculateNormals();
		points = mesh.vertices;
		
		gameObject.AddComponent<MeshCollider>();
		meshCollider = transform.GetComponent<MeshCollider>();
	}

	public virtual void ApplyMeshCollider ()
	{
		if (ToolsClass.unappliedMesh && ToolsClass.unappliedMesh.GetComponent<MeshCollider>())
		{
			meshCollider.sharedMesh = null;
			ToolsClass.unappliedMesh.GetComponent<MeshCollider>().sharedMesh = ToolsClass.unappliedMesh.sharedMesh;
		}
		ToolsClass.unappliedMesh = null;
	}
}
