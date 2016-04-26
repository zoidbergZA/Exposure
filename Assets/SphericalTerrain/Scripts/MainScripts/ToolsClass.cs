using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolsClass : EditorClass
{
	public static MeshFilter unappliedMesh;

	// Place a single prop
	public static void PlaceSingleProp (Mesh mesh, GameObject source, GameObject prop, Vector3 position, float inRadius)
	{

		Vector3 direction = (source.transform.position - position).normalized;
		Transform parent = source.transform.FindChild("Props");
		GameObject newprop = Instantiate(prop, position, Quaternion.Euler(direction)) as GameObject;
				
		newprop.transform.up = -direction;
		newprop.transform.parent = parent;
	}

	// The new group placement tool
	public static void PlaceGroupProp (Mesh mesh, GameObject source, GameObject prop, Vector3 position, float inRadius, float amount)
	{
		Vector3 lastPos = position;
		
		Vector3[] vertices = mesh.vertices;
		List<Vector3> selectedVertices = new List<Vector3> ();
		
		float sqrRadius = inRadius * inRadius;
		float sqrMagnitude;
		
		for (int i = 0; i < vertices.Length; i++)
		{
			sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			// Early out if too far away
			if (sqrMagnitude > sqrRadius)
			{
				continue;
			}

			selectedVertices.Add (vertices[i]);
		}

		for (int t = 0; t < amount; t++)
		{
			if (Vector3.Distance(lastPos, selectedVertices[t]) > PlanetStatistics.minDistance)
			{
				Vector3 direction = (source.transform.position - position).normalized;
				Transform parent = source.transform.FindChild("Props");
				GameObject newprop = Instantiate(prop, selectedVertices[Random.Range(0, selectedVertices.Count)], Quaternion.Euler(direction)) as GameObject;
				
				newprop.transform.up = -direction;
				newprop.transform.parent = parent;
				
				lastPos = selectedVertices[t];
			}
		}
	}

	public static void DeformMesh (Mesh mesh, Vector3 position, float power, float inRadius)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		float sqrRadius = inRadius * inRadius;
		float sqrMagnitude;
		float distance;
		float falloff;
		
		// Calculate averaged normal of all surrounding vertices	
		Vector3 averageNormal = Vector3.zero;
		
		for (int i = 0; i < vertices.Length; i++)
		{
			sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			// Early out if too far away
			if (sqrMagnitude > sqrRadius)
			{
				continue;
			}
			
			distance = Mathf.Sqrt(sqrMagnitude);
			falloff = LinearFalloff(distance, inRadius);
			averageNormal += falloff * normals[i];
		}
		averageNormal = averageNormal.normalized;
		
		// Deform vertices along averaged normal
		for (int i = 0; i < vertices.Length; i++)
		{
			sqrMagnitude = (vertices[i] - position).sqrMagnitude;
			// Early out if too far away
			if (sqrMagnitude > sqrRadius)
			{
				continue;
			}
				
			
			distance = Mathf.Sqrt(sqrMagnitude);
			switch (fallOff)
			{
			case FallOff.Gauss:
				falloff = GaussFalloff(distance, inRadius);
				break;
			case FallOff.Needle:
				falloff = NeedleFalloff(distance, inRadius);
				break;
			default:
				falloff = LinearFalloff(distance, inRadius);
				break;
			}
			
			vertices[i] += averageNormal * falloff * power;
		}
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
													
    public static void FlattenMesh (Transform tran, Mesh mesh, Vector3 position, float power, float altitude, float inRadius)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        float sqrRadius = inRadius * inRadius;
        float sqrMagnitude;
        float distance;
        float falloff;

        // Calculate averaged normal of all surrounding vertices	
        Vector3 averageNormal = Vector3.zero;

        for (int i = 0; i < vertices.Length; i++)
        {
            sqrMagnitude = (vertices[i] - position).sqrMagnitude;
            // Early out if too far away
            if (sqrMagnitude > sqrRadius)
			{
				continue;
			}
            
            distance = Mathf.Sqrt(sqrMagnitude);
            falloff = LinearFalloff(distance, inRadius);
            averageNormal += falloff * normals[i];
        }
        averageNormal = averageNormal.normalized;

        // Deform vertices along averaged normal
        for (int i = 0; i < vertices.Length; i++)
        {
            sqrMagnitude = (vertices[i] - position).sqrMagnitude;
            // Early out if too far away
            if (sqrMagnitude > sqrRadius)
			{
				continue;
			}
            
            distance = Mathf.Sqrt(sqrMagnitude);
            switch (fallOff)
            {
                case FallOff.Gauss:
                    falloff = GaussFalloff(distance, inRadius);
                    break;
                case FallOff.Needle:
                    falloff = NeedleFalloff(distance, inRadius);
                    break;
                default:
                    falloff = LinearFalloff(distance, inRadius);
                    break;
            }

			//Vector3 dir = (vertices[i] - tran.position ).normalized;

			if (Vector3.Distance(vertices[i], tran.position) < altitude + 1)
			{
				//vertices[i] += dir * falloff;
				//vertices[i] += averageNormal * falloff;
				vertices[i] += averageNormal * falloff * power;
				//DeformMesh(mesh, position, power, inRadius);
			}
			else if (Vector3.Distance(vertices[i], tran.position) > altitude -1)
			{
				//vertices[i] -= dir * falloff;
				//vertices[i] -= averageNormal * falloff;
				vertices[i] -= averageNormal * falloff * power;
				//DeformMesh(mesh, position, -power, inRadius);
			}
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
	
	#region Deformation Types
	public static float LinearFalloff (float distance, float inRadius)
	{
		return Mathf.Clamp01(1.0f - distance / inRadius);
	}
	
	public static float GaussFalloff (float distance , float inRadius)
	{
		return Mathf.Clamp01 (Mathf.Pow (360.0f, -Mathf.Pow (distance / inRadius, 2.5f) - 0.01f));
	}
	
	public static float NeedleFalloff (float dist, float inRadius)
	{
		return -(dist*dist) / (inRadius * inRadius) + 1.0f;
	}
	#endregion
}
