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

    public GameObject PylonsHolder;

    [SerializeField] private Transform planetTransform;
    [SerializeField] private float powerGoal = 5f;
    
    public GridBuilder GridBuilder { get; private set; }
    public Hud Hud { get; private set; }
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
        
    }

    public void AddPowerOutput(float amount)
    {
        PowerOutput += amount;
    }
}
