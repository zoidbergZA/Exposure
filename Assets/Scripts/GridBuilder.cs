﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
//    [SerializeField] private GameObject PlacerHelperPrefab;
    [SerializeField] private Pylon pylonPrefab;
    [SerializeField] private LayerMask placerMask;
    [SerializeField] private int maxPylons = 4;
//    [SerializeField] private float nodeDistance = 2.87f; // match node graph max distance variable

    private List<Connectable> ConnectedList;

    public bool ConnectionFinalized;
//    public float GridTimeLeft { get; set; }
    public GeoThermalPlant StartPlant { get; private set; }
    public List<Pylon> Pylons { get; private set; }
    public int MaxPylons { get { return maxPylons; } }
    public int PylonCount { get { return ConnectedList.Count; } }
//    public float JumpDistance { get { return nodeDistance; } }

    void Awake()
    {
        Pylons = new List<Pylon>();
    }

    void Start()
    {
//        InitPylons();
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (!succeeded && StartPlant != null)
        {
            Destroy(StartPlant.gameObject);
        }

        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
    }

    public void StartBuild(GeoThermalPlant from)
    { 
        if (IsRunning)
            return;

        Begin();
        
        StartPlant = from;
        ConnectedList = new List<Connectable>();
        ConnectionFinalized = false;
        RefreshConnectables(StartPlant.transform.position);
//        ShowUnbuiltPylons(true);
        PlaceAdjacentPylons(StartPlant.transform);
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
            PlaceAdjacentPylons(connectable.transform);

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
            
//            ShowUnbuiltPylons(false);
            Reset();
        }

//        TurnOffConnectables();
        DestroyUnbuiltPylons();
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

    private void DestroyUnbuiltPylons()
    {
        for (int i = Pylons.Count-1; i >= 0; i--)
        {
            if (Pylons[i].State == Pylon.States.Ready)
            {
                Destroy(Pylons[i].gameObject);
                Pylons.RemoveAt(i);
            }
        }
    }

    private void PlaceAdjacentPylons(Transform location)
    {
        Vector3 center = location.position + location.up*70f;

        // set raycaster positions
        Vector3[] raycastPositions = new Vector3[6];

        for (int i = 0; i < raycastPositions.Length; i++)
        {
            float period = Mathf.PI*2/raycastPositions.Length * i;
            Vector3 offset = location.forward;
            offset.x = Mathf.Sin(period);
            offset.y = Mathf.Cos(period);

            raycastPositions[i] = center + offset * GameManager.Instance.PylonSeparation;
   
            // project down from caster positions
            RaycastHit hit;

            if (Physics.Raycast(raycastPositions[i], -location.transform.up, out hit, placerMask))
            {
                //todo: sample UV map to see if spot is buildable

                //todo: check that no pylons are too close
                bool open = true;
                foreach (Pylon p in Pylons)
                {
                    if (Vector3.Distance(hit.point, p.transform.position) <= GameManager.Instance.PylonSeparation * 0.5f)
                    {
                        open = false;
                        break;
                    }
                }

                if (open)
                {
                    Pylon pylon = (Pylon) Instantiate(pylonPrefab, hit.point, location.rotation);
                    Pylons.Add(pylon);
//                    pylon.Highlight(true);
                }
            }
        }

        RefreshConnectables(location.position);
    }

//    private void TurnOffConnectables()
//    {
//        Connectable[] allConnectables = FindObjectsOfType<Connectable>();
//        
//        foreach (Connectable connectable in allConnectables)
//        {
//            connectable.TurnOff();
//        }
//    }

//    private void InitPylons()
//    {
//        for (int i = 0; i < Pylons.Length; i++)
//        {
//            for (int j = 0; j < Pylons.Length; j++)
//            {
//                if (Pylons[i] != Pylons[j])
//                {
//                    float dist = Vector3.Distance(Pylons[i].transform.position, Pylons[j].transform.position);
//                    if (dist <= nodeDistance) // match with node graph 
//                    {
//                        Pylons[i].AddConnection(Pylons[j]);
//                    }
//                }
//            }
//        }
//    }

//    private void ShowUnbuiltPylons(bool show)
//    {
//        foreach (Pylon pylon in Pylons)
//        {
//            if (pylon.State == Pylon.States.Ready)
//            {
//                pylon.ShowPlacer(show);
//            }
//        }
//    }

    private void Reset()
    {
        StartPlant = null;
        ConnectedList.Clear();
        ConnectionFinalized = false;
    }
}
