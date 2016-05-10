using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Assertions.Comparers;

public class Player : MonoBehaviour
{
    public enum PlayerStates
    {
        Drill,
        BuildGrid
    }
    
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }

    void Start()
    {
        GameManager.Instance.PylonsHolder.SetActive(false);
    }

    void Update()
    {
        switch (PlayerState)
        {
            case PlayerStates.Drill:
                HandleDrillState();
                break;
            case PlayerStates.BuildGrid:
                HandleBuildGridState();
                break;
        }
    }

    public void GoToDrillState(Transform targetTransform)
    {
        PlayerState = PlayerStates.Drill;
        GameManager.Instance.PylonsHolder.SetActive(false);
        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, targetTransform);
    }

    public void GoToBuildState(GeoThermalPlant geoPlant)
    {
        PlayerState = PlayerStates.BuildGrid;
        GameManager.Instance.PylonsHolder.SetActive(true);
        GameManager.Instance.GridBuilder.StartBuild(geoPlant);
        
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform); 
    }

    public void ScorePoints(float amount)
    {
        Score += amount;
    }

    private void HandleDrillState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, drillRayMask))
            {
                Drill(hit.point, hit.normal);
                //TestCoordExtraction(hit);
            }
        }
    }

    private void TestCoordExtraction(RaycastHit hit)
    {
        Renderer rend = hit.transform.GetComponent<Renderer>();
        Collider meshCollider = hit.collider as Collider;
        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
        {
            return;
        }

        Texture2D tex = rend.material.mainTexture as Texture2D;
        Debug.Log("yexture: " + rend.material.mainTexture.name);
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;
        for (int i = 0; i < 10; i++)
        {
            tex.SetPixel((int)pixelUV.x + i, (int)pixelUV.y + i, Color.cyan);
            tex.SetPixel((int)pixelUV.x - i, (int)pixelUV.y - i, Color.cyan);
        }
        tex.Apply();
        Debug.Log("end!");
    }

    private void HandleBuildGridState()
    {
        if (GameManager.Instance.GridBuilder.GridTimeLeft <= 0)
        {
            GameManager.Instance.GridBuilder.FinalizeGridConnection(false);
            GoToDrillState(GameManager.Instance.PlanetTransform);
            return;
        }

        GameManager.Instance.GridBuilder.GridTimeLeft -= Time.deltaTime;
    }

    private Pylon GetClosestPylon(Vector3 location)
    {
        Pylon closest = null;
        float dist = 100000f;

        foreach (Pylon pylon in GameManager.Instance.GridBuilder.Pylons)
        {
            float d = Vector3.Distance(pylon.transform.position, location);

            if (d < dist)
            {
                dist = d;
                closest = pylon;
            }
        }

        return closest;
    }

    private void Drill(Vector3 location, Vector3 normal)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.Orientate(normal);
    }
}
