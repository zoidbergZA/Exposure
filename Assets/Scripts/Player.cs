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

    }

    void Update()
    {
        switch (PlayerState)
        {
            case PlayerStates.Drill:
                HandleDrillState();
                break;
        }
    }

    public void GoToDrillState(Transform targetTransform)
    {
        PlayerState = PlayerStates.Drill;
        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, targetTransform);
    }

    public void GoToBuildState(GeoThermalPlant geoPlant)
    {
        PlayerState = PlayerStates.BuildGrid;
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
            }
        }
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
