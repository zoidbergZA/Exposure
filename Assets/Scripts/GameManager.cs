using System;
using UnityEngine;
using System.Collections;
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

//    public GameObject PylonsHolder;
    
    [SerializeField] private float roundTime = 180;
    
    
    public Planet Planet { get; private set; }
    public GridBuilder GridBuilder { get; private set; }
    public DrillingGame DrillingGame { get; private set; }
    public Scanner Scanner { get; private set; }
    public Hud Hud { get; private set; }
//    public float PylonSeparation { get { return pylonSeparation; } }
    public int TotalChimneys { get; private set; }
    public float ChimneyValue { get { return 100f/TotalChimneys; } }
    public bool RoundStarted { get; private set; }
    public float TimeLeft { get; private set; }
    public Player Player { get; private set; }
    public Director Director { get; private set; }
    public Transform PlanetTransform { get { return Planet.transform; } }

    void Awake()
    {
        Planet = FindObjectOfType<Planet>();
        GridBuilder = FindObjectOfType<GridBuilder>();
        DrillingGame = FindObjectOfType<DrillingGame>();
        Scanner = FindObjectOfType<Scanner>();
        Hud = FindObjectOfType<Hud>();
        Player = FindObjectOfType<Player>();
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

    public void Restart()
    {
        SceneManager.LoadScene(Application.loadedLevelName);
    }

    public Color SampleHeatmap(Vector2 textureCoordinate)
    {
        Material mat = Planet.scannableMesh.material;
        Texture2D heatmap = mat.GetTexture("_ScanTex") as Texture2D;
        Vector2 pixelCoord = new Vector2(textureCoordinate.x * heatmap.width, textureCoordinate.y * heatmap.height);
        Color heatmapSample = heatmap.GetPixel((int)pixelCoord.x, (int)pixelCoord.y);

        return heatmapSample;
    }

    private void StartRound()
    {
        RoundStarted = true;
        TimeLeft = roundTime;
    }

    private void EndRound()
    {
        Debug.Log("round ended! score " + Player.Score + "/100");

        Hud.GoToGameOver();
        RoundStarted = false;
    }
}
