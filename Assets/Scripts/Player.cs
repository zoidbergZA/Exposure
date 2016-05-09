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

    [SerializeField] private float gridTime = 7f;
    [SerializeField] private int maxPylons = 8;
    [SerializeField] private GameObject radar;
    [SerializeField] private float scanTime;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public bool Scanning { get; private set; }
    public float LastScan { get; private set; }
    public GeoThermalPlant ConnectingPlant { get { return connectingPlant; } }

    private float gridTimeLeft;
    private List<Connectable> ConnectedList;
    private GeoThermalPlant connectingPlant;
    private bool connectionFinalized;

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
        connectingPlant = geoPlant;
        gridTimeLeft = gridTime;
        connectionFinalized = false;
        ConnectedList = new List<Connectable>();
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform);

        RefreshConnectables(geoPlant.transform.position);
    }

    public void ScorePoints(float amount)
    {
        Score += amount;
    }

    public void AddToConnectedList(Connectable connectable)
    {
        ConnectedList.Add(connectable);
    }

    private void HandleDrillState()
    {
        if (Input.GetMouseButtonDown(1) && !Scanning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Scan(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
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
        if (gridTimeLeft <= 0)
        {
            GoToDrillState(GameManager.Instance.PlanetTransform);
            return;
        }

        gridTimeLeft -= Time.deltaTime;

        //        if (Input.GetMouseButtonDown(0) && !connectionFinalized)
        //        {
        //            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //            RaycastHit hit;
        //
        //            if (Physics.Raycast(ray, out hit, buildRayMask))
        //            {
        //                Connectable connectable = hit.transform.GetComponent<Connectable>();
        //
        //                if (connectable)
        //                {
        //                    if (connectable.IsConnectable)
        //                    {
        //                        connectable.Connect();
        //                        connectingPlant.SpanToPoint(connectable.connectionRef.position);
        //                        GameManager.Instance.Director.SetTarget(connectable.transform); 
        //
        //                        if (connectable is City)
        //                            FinalizeGridConnection(true);
        //                        else if (ConnectedList.Count >= maxPylons)
        //                        {
        //                            FinalizeGridConnection(false);
        //                        }
        //                        else
        //                        {
        //                            RefreshConnectables(connectable.transform.position);
        //                        }
        //                    }
        //                }
        //            }
        //        }


    }

    public void FinalizeGridConnection(bool succeeded)
    {
        if (succeeded)
        {
            Debug.Log("connection made! pylons used: " + ConnectedList.Count + "/" + maxPylons + ", time used: " + gridTimeLeft + "/" + gridTime);
            connectionFinalized = true;
        }
        else
        {
            Debug.Log("failed, reset pylons and wire plz");

            foreach (Connectable connectable in ConnectedList)
            {
                if (connectable is Pylon)
                {
                    Pylon pylon = (Pylon) connectable;
                    pylon.Reset();
                }
            }
            ConnectedList.Clear();
            Destroy(connectingPlant.gameObject);
        }

        gridTimeLeft = 3f;
    }

    public void RefreshConnectables(Vector3 location)
    {
        Connectable[] allConnectables = FindObjectsOfType<Connectable>();
        
        foreach (Connectable connectable in allConnectables)
        {
            connectable.CheckConnectable(location);
        }
    }

    private Pylon GetClosestPylon(Vector3 location)
    {
        Pylon closest = null;
        float dist = 100000f;

        foreach (Pylon pylon in GameManager.Instance.Pylons)
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
