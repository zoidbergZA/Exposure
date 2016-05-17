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

    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }

    void Start()
    {

    }

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

    public void ScorePoints(float amount)
    {
        Score += amount;
    }

    private void HandleNormalState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, drillRayMask))
            {
                Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
                Drill(hit.point, hit.normal, sample.r);
            }
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
