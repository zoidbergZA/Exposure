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
    [SerializeField] private float roundTime = 180;
    [SerializeField] private float pylonSeparation = 20f;
    
    public GridBuilder GridBuilder { get; private set; }
    public DrillingGame DrillingGame { get; private set; }
    public Scanner Scanner { get; private set; }
    public Hud Hud { get; private set; }
    public float PylonSeparation { get { return pylonSeparation; } }
    public int TotalChimneys { get; private set; }
    public float ChimneyValue { get { return 100f/TotalChimneys; } }
    public bool RoundStarted { get; private set; }
    public float TimeLeft { get; private set; }
    public Player Player { get; private set; }
    public Director Director { get; private set; }
    public Transform PlanetTransform { get { return planetTransform; } }

    void Awake()
    {
        GridBuilder = GetComponentInChildren<GridBuilder>();
        DrillingGame = GetComponentInChildren<DrillingGame>();
        Scanner = GetComponent<Scanner>();
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
        StartRound();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (RoundStarted)
        {
            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0)
                EndRound();
        }
    }

//    public void StartMinigame(Minigame game)
//    {
//        // todo start minigame
//    }

    private void StartRound()
    {
        RoundStarted = true;
        TimeLeft = roundTime;
    }

    private void EndRound()
    {
        Debug.Log("round ended! score " + Player.Score + "/100");

        RoundStarted = false;
    }
}
