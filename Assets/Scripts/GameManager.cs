﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
//    public enum Modes
//    {
//        Scanning,
//        DrillingGame,
//        ConnectingGame
//    }

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

    //global prefabs
    public GameObject PipePrefab;
    public Tutorial TutorialPrefab;

    public bool autoStart;
    public bool enableTutorial;
    public bool showDebug;
    public bool miniGameAutoWin;

    [SerializeField] private float roundTime = 180;
    [SerializeField] private bool touchScreenInput;
    private Tutorial tutorial;

//    public Modes Mode { get; set; }
    public bool TouchInput { get { return touchScreenInput; } set { touchScreenInput = value; } }
    public Planet Planet { get; private set; }
    public City[] Cities { get; private set; }
    public EffectsManager EffectsManager {get; private set; }
    public GridBuilder GridBuilder { get; private set; }
    public DrillingGame DrillingGame { get; private set; }
    public MobileJoystick Joystick { get; private set; }
    public Scanner Scanner { get; private set; }
    public ScannerGadget ScannerGadget { get; private set; }
    public Hud Hud { get; private set; }
    public int TotalChimneys { get; private set; }
    public float ChimneyValue { get { return 100f/TotalChimneys; } }
    public bool RoundStarted { get; private set; }
    public float TimeLeft { get; private set; }
    public Player Player { get; private set; }
    public Director Director { get; private set; }
    public Transform PlanetTransform { get { return Planet.transform; } }
    public bool MiniGameAutoWin { get { return miniGameAutoWin; } }

    void Awake()
    {
        EffectsManager = FindObjectOfType<EffectsManager>();
        Planet = FindObjectOfType<Planet>();
        GridBuilder = FindObjectOfType<GridBuilder>();
        DrillingGame = FindObjectOfType<DrillingGame>();
        Scanner = FindObjectOfType<Scanner>();
        ScannerGadget = FindObjectOfType<ScannerGadget>();
        Hud = FindObjectOfType<Hud>();
        Player = FindObjectOfType<Player>();
        Director = FindObjectOfType<Director>();
        Joystick = FindObjectOfType<MobileJoystick>();
        Cities = FindObjectsOfType<City>();

        //disable all placer scripts
        Placer[] placers = FindObjectsOfType<Placer>();

        for (int i = 0; i < placers.Length; i++)
        {
            placers[i].enabled = false;
        }

//        Mode = Modes.Scanning;

//        for (int i = 0; i < Cities.Length; i++)
//        {
//            TotalChimneys += Cities[i].ChimneyCount;
//        }

        tutorial = FindObjectOfType<Tutorial>();

        if (enableTutorial)
        {
            if (!tutorial)
                tutorial = Instantiate(TutorialPrefab);
        }
        else if (tutorial)
        {
            Destroy(tutorial.gameObject);
        }

        //disable tutorial at awake, enable at StartRound()
        if (tutorial)
            tutorial.gameObject.SetActive(false);

    }

    void Start()
    {
        Director.SetSunlightBrightness(0.2f);

        if (autoStart)
            Hud.OnStartRoundClicked();
    }

    void Update()
    {
        //cheat codes
//        if (Input.GetKeyDown(KeyCode.F3))
//            Instance.Planet.DisableNextChimney();
        if (Input.GetKeyDown(KeyCode.F8))
            TouchInput = !TouchInput;
        if (Input.GetKeyDown(KeyCode.F9))
            showDebug = !showDebug;
        //cheat codes

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }

        if (RoundStarted)
        {
            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0)
                EndRound();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(Application.loadedLevelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public Color SampleHeatmap(Vector2 textureCoordinate)
    {
        Material mat = Planet.scannableMesh.material;
        Texture2D heatmap = mat.GetTexture("_ScanTex") as Texture2D;
        Vector2 pixelCoord = new Vector2(textureCoordinate.x * heatmap.width, textureCoordinate.y * heatmap.height);
        Color heatmapSample = heatmap.GetPixel((int)pixelCoord.x, (int)pixelCoord.y);

        return heatmapSample;
    }

    public int[] LoadDrillingPuzzle(TextAsset map)
    {
        string[,] grid = CSVReader.SplitCsvGrid(map.text);
        List<int> tiles = new List<int>();

//        CSVReader.DebugOutputGrid(grid);
        
        for (int y = 0; y < grid.GetUpperBound(1); y++)
        {
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {
                int tile;
                if (int.TryParse(grid[x, y], out tile))
                {
                    tiles.Add(tile); 
                }
            }
        }
        
        return tiles.ToArray();
    }

    public void StartRound()
    {
        RoundStarted = true;
        TimeLeft = roundTime;

        if (tutorial)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.SetProgress(Tutorial.Progression.FlickPlanet);
        }
    }

    private void EndRound()
    {
        Debug.Log("round ended! score " + Player.Score + "/100");

        Hud.GoToGameOver((int)Player.Score);
        RoundStarted = false;
    }
}
