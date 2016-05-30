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

    public float flickPower = 1f;

    [SerializeField] private int startingCable = 3;
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;
    
    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public int Cable { get; private set; }

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
        if (Input.GetKeyDown(KeyCode.F2) && GameManager.Instance.Player.PlayerState == PlayerStates.Normal) // jump to drilling game
        {
            Drillspot d = Instantiate(DrillspotPrefab, Vector3.zero, Quaternion.identity) as Drillspot;
            StartDrillMinigame(d, 1f);
        }

        switch (PlayerState)
        {
            case PlayerStates.Normal:
                HandleNormalState();
                break;
        }
    }

//    void OnGUI()
//    {
//        flickPower = GUI.HorizontalSlider(new Rect(125, 155, 500, 60), flickPower, 0.2F, 9.0F);
//
//        GUI.Label(new Rect(630, 155, 60, 80), flickPower.ToString());
//    }

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
        GameManager.Instance.Director.SetMode(Director.Modes.Orbit, targetTransform);
    }

    public void StartDrillMinigame(Drillspot drillspot, float difficulty)
    {
        PlayerState = PlayerStates.DrillGame;
        GameManager.Instance.DrillingGame.StartGame(drillspot, difficulty);
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, drillspot.transform);
    }

    public void StartBuildMinigame(GeoThermalPlant geoPlant, float difficulty)
    {
        PlayerState = PlayerStates.BuildGrid;
        GameManager.Instance.GridBuilder.StartBuild(geoPlant, difficulty);
        GameManager.Instance.Director.SetMode(Director.Modes.Grid, geoPlant.transform); 
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
        HandleFlick();
    }

    private void HandleFlick()
    {
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Moved)
            {
                float deltaX = Input.touches[0].deltaPosition.x;

                GameManager.Instance.Planet.AddSpin(-deltaX * flickPower);
            }
        }
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

    private Pylon GetClosestPylon(Vector3 location)
    {
        Pylon closest = null;
        float dist = 100000f;

        foreach (Pylon pylon in GameManager.Instance.GridBuilder.Pylons)
        {
            float d = Vector3.Distance(pylon.transform.position, location);

            if (d < dist)
            {
                dist = d;
                closest = pylon;
            }
        }

        return closest;
    }

    public void Drill(Vector3 location, Vector3 normal, float difficulty)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.Orientate(normal);
        drillspot.Difficulty = difficulty;
    }
}
