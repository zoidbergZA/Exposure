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
    
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;
    [SerializeField] private float drillToastTime = 3.0f;
    [SerializeField] private UnityEngine.UI.Text testTimer;
    private float drillToastTimer;
    private bool toastMessageShown = false;
    private bool drilled = false;
    private Ray ray;
    private RaycastHit hit;
    
    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }

    void Update()
    {
        switch (PlayerState)
        {
            case PlayerStates.Normal:
                HandleNormalState();
                break;
        }
    }

    public void GoToNormalState(Transform targetTransform)
    {
        drillToastTimer = drillToastTime;
        toastMessageShown = false;
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

        if (location)
        {
            string scoreString = "+" + amount + " points!";
            GameManager.Instance.Hud.NewFloatingText(scoreString, location);
        }
    }

    private void HandleNormalState()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, drillRayMask))
                {
                    GameManager.Instance.Director.OrbitPaused = true;
                }
            }
            testTimer.text = drillToastTimer.ToString();
            drillToastTimer -= Time.deltaTime;
            if (drillToastTimer <= 0 && !drilled) toastMessageShown = true;
            if(toastMessageShown)
            {
                if (Physics.Raycast(ray, out hit, drillRayMask))
                {
                    Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
                    Drill(hit.point, hit.normal, 1f - sample.r);
                    drilled = true;
                }
                toastMessageShown = false;
            }
        }
        else
        {
            if (GameManager.Instance.Director.OrbitPaused) GameManager.Instance.Director.OrbitPaused = false;
            testTimer.text = "";
            toastMessageShown = false;
            drillToastTimer = drillToastTime;
            drilled = false;
        }
    }

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

    private void Drill(Vector3 location, Vector3 normal, float difficulty)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.Orientate(normal);
        drillspot.Difficulty = difficulty;
    }
}
