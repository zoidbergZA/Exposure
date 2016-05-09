using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (_instance == null)
                {
                    throw new Exception("no GameManager!");
                }
            }
            return _instance;
        }
    }

    public static readonly float nodeDistance = 2.87f;

    public GameObject PylonsHolder;

    [SerializeField] private Transform planetTransform;
    [SerializeField] private float powerGoal = 5f;
    
    public GridBuilder GridBuilder { get; private set; }
    public Hud Hud { get; private set; }
    public Pylon[] Pylons {get; private set; }
    public int TotalChimneys { get; private set; }
    public float ChimneyValue { get { return 100f/TotalChimneys; } }
    public float PowerOutput { get; private set; }
    public Player Player { get; private set; }
    public Director Director { get; private set; }
    public Transform PlanetTransform { get { return planetTransform; } }

    void Awake()
    {
        GridBuilder = GetComponent<GridBuilder>();
        Hud = GetComponentInChildren<Hud>();
        Pylons = FindObjectsOfType<Pylon>();
        Player = GetComponentInChildren<Player>();
        Director = FindObjectOfType<Director>();

        City[] cities = FindObjectsOfType<City>();

        for (int i = 0; i < cities.Length; i++)
        {
            TotalChimneys += cities[i].ChimneyCount;
        }
    }

    void Start()
    {
        InitPylons();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(2, 2, 100, 30), "score: " + Player.Score + "/100");
    }

    public void AddPowerOutput(float amount)
    {
        PowerOutput += amount;
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
}
