using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridBuilder : Minigame
{
    [SerializeField] private GameObject PlacerHelperPrefab;
    [SerializeField] private Pylon pylonPrefab;
    [SerializeField] private int pylonsPerPlace = 8;
    [SerializeField] private float pylonSeparation = 26f;
    [SerializeField] private LayerMask placerMask;

    public City ClosestCity { get; private set; }
    public List<Connectable> ConnectedList { get; private set; }
    public bool ConnectionFinalized;
    public GeoThermalPlant StartPlant { get; private set; }
    public float PylonSeparation { get { return pylonSeparation; } }
    public List<Pylon> Pylons { get; private set; }
    public List<Pylon> PoweredPylons { get; private set; }
    public int PylonCount { get { return ConnectedList.Count; } }

    void Awake()
    {
        Pylons = new List<Pylon>();
        PoweredPylons = new List<Pylon>();
    }

    void Start()
    {
        GameManager.Instance.Hud.ShowBuildArrow(false);
    }

    public override void Update()
    {
        base.Update();

        if (ClosestCity && GameManager.Instance.Player.PlayerState == Player.PlayerStates.BuildGrid)
        {
            Vector3 cityScreenPos = Camera.main.WorldToScreenPoint(ClosestCity.transform.position);

            Vector2 dir = (Vector2) cityScreenPos - new Vector2(Screen.width/2, Screen.height/2);
            GameManager.Instance.Hud.PointBuildArrow(dir);
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (!succeeded)
        {
            if (StartPlant != null)
                Destroy(StartPlant.gameObject);

            DestroyUnconnectedPylons();
        }
        
        DestroyUnbuiltPylons();
        
        GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        GameManager.Instance.Hud.ShowBuildArrow(false);
    }

    public void StartBuild(GeoThermalPlant from, float difficulty)
    { 
        if (IsRunning)
            return;

        Begin(difficulty);
        
        StartPlant = from;
        ConnectedList = new List<Connectable>();
        ConnectionFinalized = false;
        RefreshConnectables(StartPlant.transform.position);
        PlaceAdjacentPylons(StartPlant.transform);
        RefreshClosestCity(StartPlant.transform.position);

        GameManager.Instance.Hud.ShowBuildArrow(true);
    }

    public void MakeConnection(Connectable connectable)
    {
//        GameManager.Instance.Player.ConsumeCable(1);
        ConnectedList.Add(connectable);
//        StartPlant.SpanToPoint(connectable.connectionRef.position);

        if (ConnectedList.Count == 1)
        {
            connectable.AddConnection(StartPlant);
        }
        if (ConnectedList.Count > 1)
        {
            ConnectedList[ConnectedList.Count - 1].AddConnection(ConnectedList[ConnectedList.Count - 2]);
        }

        //todo: director jumpto()
        //        GameManager.Instance.Director.SetTarget(connectable.transform);
        Vector3 newPos = connectable.transform.position + connectable.transform.up * GameManager.Instance.Director.buildHeight;
        Quaternion newRot = Quaternion.LookRotation(connectable.transform.position - newPos, Vector3.up);

        GameManager.Instance.Director.SwoopTo(newPos, newRot, 20f, 2f);

        if (connectable is City)
        {
            FinalizeGridConnection(true);
        }
        else if (GameManager.Instance.Player.Cable <= 0)
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
//            Debug.Log("connection made! pylons used: " + ConnectedList.Count + "/" + maxPylons + ", time used: " + Timeleft + "/" + TimeOut);
            ConnectionFinalized = true;
//            StartPlant.ShowPathGuide(false);

            for (int i = 0; i < ConnectedList.Count; i++)
            {
                if (ConnectedList[i] is Pylon)
                {
                    PoweredPylons.Add((Pylon)ConnectedList[i]);
                }
            }

//            float points = GameManager.Instance.ChimneyValue;
            float points = 20;

            GameManager.Instance.Player.ScorePoints(points, ConnectedList[ConnectedList.Count - 1].transform);
        }
        else
        {
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
//        DestroyUnbuiltPylons();
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

    private void DestroyUnconnectedPylons()
    {
        for (int i = Pylons.Count - 1; i >= 0; i--)
        {
            if (Pylons[i].State == Pylon.States.Built)
            {
                Destroy(Pylons[i].gameObject);
                Pylons.RemoveAt(i);
            }
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
        Vector3 dir = (GameManager.Instance.PlanetTransform.position - location.position).normalized;
        Vector3 center = location.position + dir*-130f;

//        GameObject placer = (GameObject)Instantiate(PlacerHelperPrefab, center, Quaternion.identity);
//        placer.transform.forward = dir;
        DestroyUnbuiltPylons();
        // set raycaster positions
        Vector3[] raycastPositions = new Vector3[pylonsPerPlace];

        for (int i = 0; i < raycastPositions.Length; i++)
        {
            float period = Mathf.PI*2/raycastPositions.Length * i;
            Vector3 offset = new Vector3(Mathf.Sin(period), Mathf.Cos(period), 0);

            Quaternion q = Quaternion.LookRotation(dir);

            raycastPositions[i] = center + q*offset * pylonSeparation;

            // project down from caster positions
            RaycastHit hit;
            
            if (Physics.Raycast(raycastPositions[i], dir, out hit, placerMask))
            {
                Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
//                Debug.Log(sample);
                //check that no pylons are too close
                bool open = true;
                foreach (Pylon p in Pylons)
                {
                    if (Vector3.Distance(hit.point, p.transform.position) <= pylonSeparation * 0.5f)
                    {
                        open = false;
                        break;
                    }
                }

                if (open && sample.g >= 0.1f)
                {
                    Pylon pylon = (Pylon) Instantiate(pylonPrefab, hit.point, location.rotation);
                    pylon.transform.SetParent(GameManager.Instance.PlanetTransform);
                    Pylons.Add(pylon);
                    
                    pylon.transform.up = hit.normal;
//                    pylon.Highlight(true);
                }
            }
        }

        RefreshConnectables(location.position);
    }

    private void RefreshClosestCity(Vector3 position)
    {
        ClosestCity = FindClosestOpenCity(position);
    }

    private City FindClosestOpenCity(Vector3 position)
    {
        City closestCity = null;
        float distance = Mathf.Infinity;

        for (int i = 0; i < GameManager.Instance.Cities.Length; i++)
        {
            float d = Vector3.Distance(position, GameManager.Instance.Cities[i].transform.position);
            if (GameManager.Instance.Cities[i].HasWorkingChimney && d < distance)
            {
                closestCity = GameManager.Instance.Cities[i];
                distance = d;
            }
        }

        return closestCity;
    }

    private void Reset()
    {
        StartPlant = null;
        ConnectedList.Clear();
        ConnectionFinalized = false;
    }
}
