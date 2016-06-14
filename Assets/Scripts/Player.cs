using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Assertions.Comparers;

public class Player : MonoBehaviour
{
    public enum PlayerStates
    {
        Normal,
        DrillGame,
        BuildGrid
    }

    public delegate void FlickHandler();

    public static event FlickHandler PlanetFlicked;

    public float flickPower = 1f;

    [SerializeField] private int startingCable = 3;
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public string PlayerName { get; private set; }
    public int PlayerAge { get; private set; }
    public bool PlayerIsMale { get; private set; }
    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public int Cable { get; private set; }

    private Vector2 mouseOld;

    void Awake()
    {
        Cable = startingCable;
    }

    void Update()
    {
        //temp
        if (Input.GetKeyDown(KeyCode.F1))
            GameManager.Instance.Restart();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CollectCable(1);
        if (Input.GetKeyDown(KeyCode.F6) && GameManager.Instance.Player.PlayerState == PlayerStates.Normal) // jump to drilling game
        {
            GameManager.Instance.Player.StartDrillMinigame(GameManager.Instance.Cities[0].PuzzlePath.GeoPlant, 1f);
        }

        switch (PlayerState)
        {
            case PlayerStates.Normal:
                HandleNormalState();
                break;
        }
    }

    void LateUpdate()
    {
        mouseOld = Input.mousePosition;
    }

    public void SetPlayerInfo(string name, int age, bool isMale)
    {
        PlayerName = name;
        PlayerAge = age;
        PlayerIsMale = isMale;
    }

    public void CollectCable(int amount)
    {
        Cable += amount;
        GameManager.Instance.Hud.ShakeCablePanel();
    }

    public void ConsumeCable(int amount)
    {
        Cable -= amount;
        GameManager.Instance.Hud.ShakeCablePanel();
    }

    public void GoToNormalState(Transform targetTransform)
    {
        PlayerState = PlayerStates.Normal;
        EnableRadar(true);
        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, targetTransform);
    }

    public void StartDrillMinigame(GeoThermalPlant geoPlant, float difficulty)
    {
        PlayerState = PlayerStates.DrillGame;
        EnableRadar(false);
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform, 2f);

        //set the puzzle of this geoplant as the next gridBuilder puzzle
        GameManager.Instance.GridBuilder.SetPuzzlePath(geoPlant.PuzzlePath);
        GameManager.Instance.DrillingGame.StartGame(null, difficulty);
    }

    public void StartBuildMinigame(GeoThermalPlant geoPlant, float difficulty)
    {
//        PlayerState = PlayerStates.BuildGrid;
//        GameManager.Instance.GridBuilder.StartBuild(geoPlant, difficulty);
//        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform); 
    }

    public void EnableRadar(bool enable)
    {
        GameManager.Instance.Scanner.ShowTerrainScanner(enable);
        GameManager.Instance.Scanner.gameObject.SetActive(enable);
        GameManager.Instance.ScannerGadget.gameObject.SetActive(enable);
        
    }

    public void ScorePoints(float amount, Transform location = null)
    {
        Score += amount;
        GameManager.Instance.Hud.ShakeScorePanel();

        if (location)
        {
            string scoreString = "+" + amount + " points!";
            GameManager.Instance.Hud.NewFloatingText(scoreString, location);
        }
    }

    private void HandleNormalState()
    {
        CheckPlanetFlick();
    }

    private void CheckPlanetFlick()
    {
        if (GameManager.Instance.ScannerGadget.IsGrabbed)
            return;

        float deltaX = 0;

        if (GameManager.Instance.TouchInput)
        {
            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Moved)
                    deltaX = Input.touches[0].deltaPosition.x;
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
                deltaX = Input.mousePosition.x - mouseOld.x;
        }

        if (Mathf.Abs(deltaX) > 2)
            HandlePlanetFlick(deltaX);
    }

    private void HandlePlanetFlick(float deltaX)
    {
        GameManager.Instance.Planet.AddSpin(-deltaX * flickPower);

        if (PlanetFlicked != null)
            PlanetFlicked();
    }

//    private void HandleNormalState()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            RaycastHit hit;
//
//            if (Physics.Raycast(ray, out hit, drillRayMask))
//            {
//                Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
//                Drill(hit.point, hit.normal, 1f - sample.r);
//            }
//        }
//    }

//    private Pylon GetClosestPylon(Vector3 location)
//    {
//        Pylon closest = null;
//        float dist = 100000f;
//
//        foreach (Pylon pylon in GameManager.Instance.GridBuilder.Pylons)
//        {
//            float d = Vector3.Distance(pylon.transform.position, location);
//
//            if (d < dist)
//            {
//                dist = d;
//                closest = pylon;
//            }
//        }
//
//        return closest;
//    }

    public void Drill(Vector3 location, Vector3 normal, float difficulty)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.transform.SetParent(GameManager.Instance.PlanetTransform);
        drillspot.Orientate(normal);
        drillspot.Difficulty = difficulty;
    }
}
