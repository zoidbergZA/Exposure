﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    //global prefabs
    public TapTips TapTipsPrefab;
    public GameObject PipePrefab;
    public Tutorial TutorialPrefab;

    public bool autoStart;
    public bool skipIntro;
    public bool enableTutorial;
    public bool showDebug;
    public bool miniGameAutoWin;

    [SerializeField] private City[] cities;
    [SerializeField] private float roundTime = 180;
    [SerializeField] private bool touchScreenInput;

    private Tutorial tutorial;

	public Arguments HeimArguments { get { return GetComponent<Arguments> (); } }
	public DBconnection HeimDbConnection { get { return GetComponent<DBconnection> (); } }
    public bool TouchInput { get { return touchScreenInput; } set { touchScreenInput = value; } }
    public Intro Intro { get; private set; }
    public Planet Planet { get; private set; }
    public City[] Cities { get { return cities; } }
    public TapTips TapTips { get; private set; }
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
        Intro = GetComponent<Intro>();
        Planet = FindObjectOfType<Planet>();
        TapTips = Instantiate(TapTipsPrefab);
        GridBuilder = FindObjectOfType<GridBuilder>();
        DrillingGame = FindObjectOfType<DrillingGame>();
        Scanner = FindObjectOfType<Scanner>();
        ScannerGadget = FindObjectOfType<ScannerGadget>();
        Hud = FindObjectOfType<Hud>();
        Player = FindObjectOfType<Player>();
        Director = FindObjectOfType<Director>();
        Joystick = FindObjectOfType<MobileJoystick>();

        //disable all placer scripts
        Placer[] placers = FindObjectsOfType<Placer>();

        for (int i = 0; i < placers.Length; i++)
        {
            placers[i].enabled = false;
        }

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
        LoadHeimInfo();

        Director.SetSunlightBrightness(true);

        if (autoStart)
            Hud.OnStartRoundClicked();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            TouchInput = !TouchInput;

//        if (Input.GetMouseButtonDown(0))
//            TouchInput = false;
//        else if (Input.touchCount > 0)
//            TouchInput = true;

        //cheat codes
        if (Input.GetKeyDown(KeyCode.F8))
            TouchInput = !TouchInput;
        if (Input.GetKeyDown(KeyCode.F9))
            showDebug = !showDebug;
        if (Input.GetKeyDown(KeyCode.F4))
            CleanNextCity();
        if (Input.GetKeyDown(KeyCode.F12))
            Restart();
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

    public void HideAllGeoPlantPreviews()
    {
        foreach (City city in Cities)
        {
            city.PuzzlePath.GeoPlant.ShowPreview(false);
        }
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
        Planet.normalSpin = 0;
        Player.LastInputAt = Time.time;
        Instance.Hud.OnRoundStarted();

        if (tutorial)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.SetProgress(Tutorial.Progression.FlickPlanet);
        }
    }

    public void EndRound()
    {
//        Debug.Log("round ended! score " + Player.Score + "/100");
        
        if (DrillingGame.IsRunning)
            DrillingGame.End(false);
        if (GridBuilder.IsRunning)
            GridBuilder.End(false);

        Player.EnableRadar(false, Vector3.zero);
        Planet.normalSpin = 8f;

        Hud.GoToGameOver((int)Player.Score);
        RoundStarted = false;

        SendHeimData();
    }

    public void HandleTimeOut()
    {
        //Restart();
        Application.Quit();
    }

    private void LoadHeimInfo()
    {
		if (HeimArguments.getGameTime() > 0)
        {
			roundTime = HeimArguments.getGameTime() - 10f;
        }

        ScannerGadget.SetGender(true);
        
        //assuming 0 is male and 1 is female
        DrillingGame.Driller.Gender = (Driller.DrillerGender)0;
        DrillingGame.Driller.SetGenderAttributes(DrillingGame.Driller.Gender);
    }

    private void SendHeimData()
    {
		StartCoroutine(HeimDbConnection.UploadScore (HeimArguments.getUserID(), HeimArguments.getGameID(), (int)Player.Score));

        //string requestString = "insertScore.php?userID=" + heimPlayerData.userID + "&gameID=" + heimPlayerData.gameID + "&score=" + Player.Score;

        //string url = heimPlayerData.conURL + requestString;
        //WWW www = new WWW(url);
        //StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.data);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    private void CleanNextCity()
    {
        foreach (City city in Cities)
        {
            if (city.CityState == CityStates.DIRTY)
            {
                city.CleanUp();
                break;
            }
        }
    }
}
