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

    [SerializeField] private int startingCable = 3;
    [SerializeField] private LayerMask drillRayMask;
    [SerializeField] private LayerMask buildRayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;
    [SerializeField] private float drillToastTime = 1.0f;

    private float drillToastTimer;
    private bool toastMessageShown = false;
    private bool drilled = false;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 initPressureImagePos;
    private Vector3 initBgImagePos;
    
    public PlayerStates PlayerState { get; private set; }
    public float Score { get; private set; }
    public int Cable { get; private set; }

    void Awake()
    {
        Cable = startingCable;
    }

    void Start()
    {
        initPressureImagePos = GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition;
        initBgImagePos = GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition;
    }

    void Update()
    {
        //temp
        if (Input.GetKeyDown(KeyCode.F1))
            GameManager.Instance.Restart();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CollectCable(1);
        //temp

        switch (PlayerState)
        {
            case PlayerStates.Normal:
                HandleNormalState();
                break;
        }
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
        GameManager.Instance.Hud.ShakeScorePanel();

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
                    GameManager.Instance.DrillingGame.PressureIcon.rectTransform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y+140, Input.mousePosition.z);

                    GameManager.Instance.Scanner.StartScan(hit.point);
                }
                activateImages(true);
            }

            moveUImages();

            drillToastTimer -= Time.deltaTime;
            if (drillToastTimer <= 0 && !drilled) toastMessageShown = true;
            if(toastMessageShown)
            {
                if (Physics.Raycast(ray, out hit, drillRayMask))
                {
                    Color sample = GameManager.Instance.SampleHeatmap(hit.textureCoord);
                    Drill(hit.point, hit.normal, 1f - sample.r);
                    drilled = true;
                    activateImages(false);
                }
                toastMessageShown = false;
            }
        }
        else
        {
            if (GameManager.Instance.Director.OrbitPaused) GameManager.Instance.Director.OrbitPaused = false;
            toastMessageShown = false;
            drillToastTimer = drillToastTime;
            drilled = false;
            activateImages(false);
            GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition = initPressureImagePos;
            GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition = initBgImagePos;

            GameManager.Instance.Scanner.EndScan();
        }
    }

    private float getUpSpeed(float distance)
    {
        float result = distance / drillToastTimer;
        return result;
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

    private void moveUImages()
    {
        if (GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.anchoredPosition.y < GameManager.Instance.DrillingGame.StartToast.rectTransform.anchoredPosition.y)
            GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.Translate(0,
                getUpSpeed(GameManager.Instance.DrillingGame.StartInnerToast.rectTransform.rect.height / 2) * Time.deltaTime, 0);

        if (GameManager.Instance.DrillingGame.BgActive.rectTransform.anchoredPosition.y < initBgImagePos.y + GameManager.Instance.DrillingGame.BgActive.rectTransform.rect.height)
            GameManager.Instance.DrillingGame.BgActive.rectTransform.Translate(0,
                getUpSpeed(GameManager.Instance.DrillingGame.BgActive.rectTransform.rect.height / 2) * Time.deltaTime, 0);
    }

    private void activateImages(bool activate)
    {
        GameManager.Instance.DrillingGame.StartToast.gameObject.SetActive(activate);
        GameManager.Instance.DrillingGame.StartInnerToast.gameObject.SetActive(activate);
        GameManager.Instance.DrillingGame.BgActive.gameObject.SetActive(activate);
    }
}
