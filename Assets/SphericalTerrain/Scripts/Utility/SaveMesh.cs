#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public class SaveMesh : MonoBehaviour
{
    public GameObject meshToSave;
	private string path = "Assets/SphericalTerrain/Resources/Meshes/SavedMeshes/";
	private string extension = ".asset";

	void Start () {}
    void Update () {}

	public void SaveAsset(string fileName)
	{
		Mesh mesh1 = meshToSave.transform.GetComponent<MeshFilter>().mesh;
		AssetDatabase.CreateAsset(mesh1, path + fileName + extension);
		Debug.Log (fileName + extension + " saved to " + path);
	}
	
}
#endif