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

    
    [SerializeField] private GameObject radar;
    [SerializeField] private float scanTime;
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public bool Scanning { get; private set; }
    public float LastScan { get; private set; }

    void Start()
    {
        GameManager.Instance.PylonsHolder.SetActive(false);
    }

    void Update()
    {
        if (Scanning && Time.time > LastScan + scanTime)
            Scanning = false;

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
        if (Input.GetMouseButtonDown(1) && !Scanning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, drillRayMask))
            {
                Scan(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, drillRayMask))
            {
                Drill(hit.point, hit.normal);
            }
        }

        if (Scanning)
        {
            radar.SetActive(true);
        }
        else
        {
            radar.SetActive(false);
        }
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

    private void Scan(Vector3 location)
    {
        LastScan = Time.time;
        Scanning = true;
        radar.transform.position = location;
    }
}
