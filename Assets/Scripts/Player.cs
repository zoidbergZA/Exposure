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
    
    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public int Cable { get; private set; }
    public float LastInputAt { get; set; }

    private Vector2 mouseOld;

    void Awake()
    {
        Cable = startingCable;

//        //todo: temp
//        PlayerName = "Juanito";
    }

    void Start()
    {
        EnableRadar(false, GameManager.Instance.ScannerGadget.transform.position);
    }

    void Update()
    {
        if (!GameManager.Instance.RoundStarted)
            return;

        //check input timeout
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            LastInputAt = Time.time;
        
        if (Time.time > LastInputAt + 30f)
            GameManager.Instance.HandleTimeOut();

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
        if (Input.mousePosition != Vector3.zero)
            mouseOld = Input.mousePosition;
        if (Input.touchCount > 0)
            mouseOld = Input.touches[0].position;
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

    public void GoToNormalState(Vector3 scannerPosition)
    {
        PlayerState = PlayerStates.Normal;
        EnableRadar(true, scannerPosition);
        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, GameManager.Instance.PlanetTransform);
    }

    public void StartDrillMinigame(GeoThermalPlant geoPlant, float difficulty)
    {
        PlayerState = PlayerStates.DrillGame;
        EnableRadar(false, geoPlant.transform.position);
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform, 2f);
        
        //set the puzzle of this geoplant as the next gridBuilder puzzle
        GameManager.Instance.GridBuilder.SetPuzzlePath(geoPlant.PuzzlePath);

        StartCoroutine(StartDrillGameAfter(2.4f, geoPlant));
    }

//    public void StartBuildMinigame(GeoThermalPlant geoPlant, float difficulty)
//    {
////        PlayerState = PlayerStates.BuildGrid;
////        GameManager.Instance.GridBuilder.StartBuild(geoPlant, difficulty);
////        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform); 
//    }

    public void EnableRadar(bool enable, Vector3 position)
    {
        GameManager.Instance.ScannerGadget.transform.position = position;

//        GameManager.Instance.Scanner.ShowTerrainScanner(enable);
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

    private IEnumerator StartDrillGameAfter(float seconds, GeoThermalPlant geoPlant)
    {
        yield return new WaitForSeconds(seconds);

        GameManager.Instance.Director.SetSunlightBrightness(true);
        GameManager.Instance.Hud.ShowStatusPanel(false);
        GameManager.Instance.DrillingGame.StartGame(null, 1f);
    }

    private void HandleNormalState()
    {
        CheckPlanetFlick();
    }

    private void CheckPlanetFlick()
    {
        if (GameManager.Instance.ScannerGadget.IsGrabbed)
            return;

                if (GameManager.Instance.TouchInput && Input.touchCount == 0)
                    return;
                if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
                    return;
                
        Vector2 inputPos = Vector2.zero;
        float deltaX = 0;

        if (GameManager.Instance.TouchInput)
        {
            inputPos = Input.touches[0].position;
            if (Input.touches[0].phase == TouchPhase.Moved)
                deltaX = Input.touches[0].deltaPosition.x;
        }
        else
        {
            inputPos = Input.mousePosition;
            deltaX = Input.mousePosition.x - mouseOld.x;
        }

        Ray ray = Camera.main.ScreenPointToRay(inputPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "Planet")
            {
                if (Mathf.Abs(deltaX) > 0.02f)
                    HandlePlanetFlick(deltaX);
            }
        }
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
