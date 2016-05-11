using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
//    [SerializeField] private float gridTime = 15f;
    [SerializeField] private int maxPylons = 4;
    [SerializeField] private float nodeDistance = 2.87f; // match node graph max distance variable

    private List<Connectable> ConnectedList;

    public bool ConnectionFinalized;
//    public float GridTimeLeft { get; set; }
    public GeoThermalPlant StartPlant { get; private set; }
    public Pylon[] Pylons { get; private set; }
    public int MaxPylons { get { return maxPylons; } }
    public int PylonCount { get { return ConnectedList.Count; } }
    public float JumpDistance { get { return nodeDistance; } }

    void Awake()
    {
        Pylons = FindObjectsOfType<Pylon>();
    }

    void Start()
    {
        InitPylons();
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        GameManager.Instance.Player.GoToDrillState(GameManager.Instance.PlanetTransform);
    }

    public void StartBuild(GeoThermalPlant from)
    { 
        Begin();
        
        StartPlant = from;
        ConnectedList = new List<Connectable>();
        ConnectionFinalized = false;
        RefreshConnectables(StartPlant.transform.position);
        ShowUnbuiltPylons(true);
    }

    public void MakeConnection(Connectable connectable)
    {
        ConnectedList.Add(connectable);
        StartPlant.SpanToPoint(connectable.connectionRef.position);
        GameManager.Instance.Director.SetTarget(connectable.transform);

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
        if (ConnectionFinalized)
            return;

        if (succeeded)
        {
            Debug.Log("connection made! pylons used: " + ConnectedList.Count + "/" + maxPylons + ", time used: " + Timeleft + "/" + TimeOut);
            ConnectionFinalized = true;
            StartPlant.ShowPathGuide(false);
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.ChimneyValue / MaxPylons * (MaxPylons - PylonCount + 1));
        }
        else
        {
            Debug.Log("failed");
            if (StartPlant != null)
            {
                Destroy(StartPlant.gameObject);
            }

            foreach (Connectable connectable in ConnectedList)
            {
                if (connectable is Pylon)
                {
                    Pylon pylon = (Pylon) connectable;
                    pylon.Reset();
                }
            }
            
            ShowUnbuiltPylons(false);
            Reset();
        }

        TurnOffConnectables();
        End(succeeded);
    }

    public void RefreshConnectables(Vector3 location)
    {
        Connectable[] allConnectables = FindObjectsOfType<Connectable>();

        foreach (Connectable connectable in allConnectables)
        {
            connectable.CheckConnectable(location);
        }
    }

    private void TurnOffConnectables()
    {
        Connectable[] allConnectables = FindObjectsOfType<Connectable>();
        
        foreach (Connectable connectable in allConnectables)
        {
            connectable.TurnOff();
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

    private void ShowUnbuiltPylons(bool show)
    {
        foreach (Pylon pylon in Pylons)
        {
            if (pylon.State == Pylon.States.Ready)
            {
                pylon.ShowPlacer(show);
            }
        }
    }

    private void Reset()
    {
        StartPlant = null;
        ConnectedList.Clear();
        ConnectionFinalized = false;
    }
}
