using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : MonoBehaviour
{
    [SerializeField] private float gridTime = 15f;
    [SerializeField] private int maxPylons = 4;
    [SerializeField] private float nodeDistance = 2.87f; // match node graph max distance variable

    private List<Connectable> ConnectedList;
    private bool connectionFinalized;

    public float GridTimeLeft { get; set; }
    public GeoThermalPlant StartPlant { get; private set; }
    public Pylon[] Pylons { get; private set; }
    public float JumpDistance { get { return nodeDistance; } }

    void Awake()
    {
        Pylons = FindObjectsOfType<Pylon>();
    }

    void Start()
    {
        InitPylons();
    }

    public void StartBuild(GeoThermalPlant from)
    {
        GridTimeLeft = gridTime;
        StartPlant = from;
        ConnectedList = new List<Connectable>();
        connectionFinalized = false;
        RefreshConnectables(StartPlant.transform.position);
    }

    public void MakeConnection(Connectable connectable)
    {
        ConnectedList.Add(connectable);
        StartPlant.SpanToPoint(connectable.connectionRef.position);
        GameManager.Instance.Director.SetTarget(connectable.transform);
        RefreshConnectables(transform.position);

        //check completetion conditions
        if (connectable is City)
        {
            FinalizeGridConnection(true);
        }
        else if (ConnectedList.Count >= maxPylons)
        {
            FinalizeGridConnection(false);
        }
        else
        {
            RefreshConnectables(connectable.transform.position);
        }
    }

    public void FinalizeGridConnection(bool succeeded)
    {
        if (succeeded)
        {
            Debug.Log("connection made! pylons used: " + ConnectedList.Count + "/" + maxPylons + ", time used: " + GridTimeLeft + "/" + gridTime);
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

            Reset();
        }

        GameManager.Instance.GridBuilder.GridTimeLeft = 2f;
    }

    public void RefreshConnectables(Vector3 location)
    {
        Connectable[] allConnectables = FindObjectsOfType<Connectable>();

        foreach (Connectable connectable in allConnectables)
        {
            connectable.CheckConnectable(location);
        }
    }

    private void InitPylons()
    {
        for (int i = 0; i < Pylons.Length; i++)
        {
            for (int j = 0; j < Pylons.Length; j++)
            {
                if (Pylons[i] != Pylons[j])
                {
                    float dist = Vector3.Distance(Pylons[i].transform.position, Pylons[j].transform.position);
                    if (dist <= nodeDistance) // match with node graph 
                    {
                        Pylons[i].AddConnection(Pylons[j]);
                    }
                }
            }
        }
    }

    private void Reset()
    {
        if (StartPlant != null)
        {
            Destroy(StartPlant.gameObject);
        }

        ConnectedList.Clear();
        connectionFinalized = false;
    }
}
