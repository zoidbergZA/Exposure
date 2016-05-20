﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrillingGame : Minigame
{
    //temp - jacques test
    public AnimationCurve RocksCurve;
    public AnimationCurve CrystalsCurve;

    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private UnityEngine.UI.Image pressureIcon;
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Text timer;
    [SerializeField] private UnityEngine.UI.Image startToast;
    [SerializeField] private UnityEngine.UI.Image startInnerToast;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image endFailToast;
    [SerializeField] private int[] columns;
    [SerializeField] private int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject canvas;
    [SerializeField] private bool AutoWin;
    [SerializeField] private float toastMessageTime = 3.0f;
    [SerializeField] public float stuckTime = 10.0f;
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float succeededDrillValue = 5.0f;
    [SerializeField] private float drillStuckCooldown = 2.0f;
    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST }
    private DrillingGameState state;
    private bool makeDrill = false;
    private Vector3 initDrillPos;
    private int targetColumn;
    private int targetRow;
    private float toastTimer;
    private bool slidingLeft = false;
    private bool introShown, finalShown = false;
    private bool imagesActivated = false;
    private float drillStuckChecked;
    private float stuckTimer;

    public bool succeededDrill { get; set; }
    private List<GameObject> rocks = new List<GameObject>();
    public DrillingGameState State { get { return state; } set { state = value; } }
    public void SetMakeDrill(bool value) { makeDrill = value; }
    public UnityEngine.UI.Image GetDrill { get { return drill; } }
    public float DiamondValue { get { return diamondValue; } }
    public float ToastTimer { get { return toastTimer; } set { toastTimer = value; } }
    public UnityEngine.UI.Image StartToast { get { return startToast; } }
    public UnityEngine.UI.Image StartInnerToast { get { return startInnerToast; } }
    public UnityEngine.UI.Image BgActive { get { return bgActive; } }
    public UnityEngine.UI.Image PressureIcon { get { return pressureIcon; } }
    public bool MovingLeft { get; set; }
    public bool MovingRight { get; set; }
    public float StuckTimer { get { return stuckTimer; } set { stuckTimer = value; } }
    public bool ReachedBottom(int bottom, UnityEngine.UI.Image drill)
    {
        return drill.rectTransform.anchoredPosition.y <= initDrillPos.y - bottom;
    }
    

    void Start()
    {
        activateImages(false);
        if (drill) initDrillPos = drill.rectTransform.anchoredPosition;
        targetColumn = 0;
        targetRow = 0;
        toastTimer = toastMessageTime;
        if(mainPanel) mainPanel.rectTransform.position = new Vector3((Screen.width / 3) / 2, Screen.height / 2, 0);
        if (startInnerToast && startToast) startInnerToast.transform.SetSiblingIndex(startToast.transform.GetSiblingIndex() - 1);
        drillStuckChecked = Time.time;
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        if (!AutoWin) state = DrillingGameState.SLIDING;
        else state = DrillingGameState.SUCCESS;
        imagesActivated = true;
        introShown = true;
        generateMap();
        if (bgActive) bgActive.rectTransform.anchoredPosition = new Vector3(0, -23, 0);
        drill.transform.SetAsLastSibling();
    }

    private void generateMap()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < rows.Length-1; j++)
            {
                if (j == 0) instantiateGroundTile(columns[i], rows[j]);
                else
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand <= RocksCurve.Evaluate(1 - Difficulty)) instantiateCable(columns[i], rows[j]); //try cable
                    else
                    {
                        rand = Random.Range(0f, 1f);
                        if (rand <= RocksCurve.Evaluate(1 - Difficulty)) instantiateRock(columns[i], rows[j]); //try rock
                        else
                        {
                            rand = Random.Range(0f, 1f); //else try daimond
                            if (rand <= CrystalsCurve.Evaluate(1 - Difficulty)) instantiateDiamond(columns[i], rows[j]);
                            else instantiateGroundTile(columns[i], rows[j]);
                        }
                    }
                }
            }
        }
    }

    private void updateState()
    {
        switch(state)
        {
            case DrillingGameState.DRILLING:
                handleDrillingState();
                break;
            case DrillingGameState.SLIDING:
                handleSlidingState();
                break;
            case DrillingGameState.INACTIVE:
                handleInactiveState();
                break;
            case DrillingGameState.SUCCESS:
                handleSuccessState();
                break;
            case DrillingGameState.STARTSTOPTOAST:
                handleStartStopState();
                break;
        }
    }

    private void handleStartStopState()
    {
        toastTimer -= Time.deltaTime;
        if(introShown && !finalShown)
        {
            if (succeededDrill)
            {
                endOkToast.gameObject.SetActive(true);
                endOkToast.transform.SetAsLastSibling();
                if (toastTimer < 0.0f)
                {
                    finalShown = true;
                    toastTimer = toastMessageTime;
                    endOkToast.gameObject.SetActive(false);
                    state = DrillingGameState.INACTIVE;
                    End(true);
                }
            }
            else
            {
                endFailToast.gameObject.SetActive(true);
                endFailToast.transform.SetAsLastSibling();
                if (toastTimer < 0.0f)
                {
                    finalShown = true;
                    toastTimer = toastMessageTime;
                    endFailToast.gameObject.SetActive(false);
                    End(false);
                    state = DrillingGameState.INACTIVE;
                }   
            }
        }
    }

    private void handleDrillingState()
    {
        if (!ReachedBottom(495, drill)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void updateDrilling()
    {
        if (!MovingRight && !MovingLeft)
        {
            drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0);
            if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y + 518 <= rows[targetRow + 1]) targetRow++;
        }
        else
        {
            if(MovingRight)
            {
                if (targetColumn < columns.Length - 1)
                {
                    //if (drill.rectTransform.anchoredPosition.y + 518 >= rows[targetRow + 1]) drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0); //drill down
                    drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
                    if (drill.rectTransform.anchoredPosition.x >= columns[targetColumn + 1]) targetColumn += 1;
                }
                else MovingRight = false;
            }
            else if(MovingLeft)
            {
                if (targetColumn > 0)
                {
                    //if (drill.rectTransform.anchoredPosition.y + 518 >= rows[targetRow + 1]) drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0); //drill down
                    drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
                    if (drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1]) targetColumn -= 1;
                }
                else MovingLeft = false;
            }
        }
//        Debug.Log("right: " + MovingRight + " | left: " + MovingLeft);
    }

    public void MoveRight()
    {
        MovingRight = true;
        MovingLeft = false;
    }

    public void MoveLeft()
    {
        MovingRight = false;
        MovingLeft = true;
    }

    private void handleSlidingState()
    {
        activateImages(true);
        updateSlidingMovement();
    }

    private void updateSlidingMovement()
    {
        if (!makeDrill)
        {
            if (slidingLeft == false)
            {
                drill.transform.Translate(new Vector3(1 * slideSpeed * Time.deltaTime, 0, 0));
                if (targetColumn < columns.Length - 1)
                {
                    if (drill.rectTransform.anchoredPosition.x >= columns[targetColumn + 1]) targetColumn += 1;
                }
                else slidingLeft = true;
            }
            else
            {
                drill.transform.Translate(new Vector3(-1 * slideSpeed * Time.deltaTime, 0, 0));
                if (targetColumn > 0)
                {
                    if (drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1]) targetColumn -= 1;
                }
                else slidingLeft = false;
            }
        }
        else
        {
            if (slidingLeft == false)
            {
                if (drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1])
                    drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn + 1], drill.rectTransform.anchoredPosition.y);
                else
                {
                    state = DrillingGameState.DRILLING;
                    if (targetColumn < columns.Length - 1) targetColumn += 1;
                }
            }
            else
            {
                if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                    drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn - 1], drill.rectTransform.anchoredPosition.y);
                else
                {
                    state = DrillingGameState.DRILLING;
                    if (targetColumn > 0) targetColumn -= 1;
                }
            }
        }
    }

    private void handleInactiveState()
    {
        if (imagesActivated) activateImages(false);
    }

    private void handleSuccessState()
    {
        succeededDrill = true;
        state = DrillingGameState.STARTSTOPTOAST;
    }

    public override void Update()
    {
        base.Update();
        if(drill) updateState();
        if (IsRunning && Timeleft <= 0.5f) End(false);
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && state != DrillingGameState.INACTIVE)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
        }
        if (state != DrillingGameState.INACTIVE && state != DrillingGameState.STARTSTOPTOAST)
        {
            timer.text = ((int)Timeleft).ToString();
            timer.color = Color.Lerp(Color.red, Color.green, Timeleft/TimeOut);
        }

        if (Time.time - drillStuckChecked > drillStuckCooldown)
        {
            checkDrillerStuck();
            drillStuckChecked = Time.time;
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            Destroy(drillspot.gameObject);
            GameManager.Instance.Player.StartBuildMinigame(plant, 1f);
            GameManager.Instance.Player.ScorePoints(succeededDrillValue);
        }
        else GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        
        resetGameGuts();
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (bgActive) bgActive.gameObject.SetActive(true);
            if (drill) drill.gameObject.SetActive(true);
            if (timer) timer.gameObject.SetActive(true);
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
            if (drill) drill.gameObject.SetActive(false);
            if (timer) timer.gameObject.SetActive(false);
            imagesActivated = false;
        }
    }

    private void resetGameGuts()
    {
        makeDrill = false;
        slidingLeft = false;
        introShown = false;
        finalShown = false;
        succeededDrill = false;
        targetColumn = 0;
        targetRow = 0;
        foreach (GameObject rock in rocks) Destroy(rock);
        drill.rectTransform.anchoredPosition = initDrillPos;
        rocks.Clear();
    }

    private void instantiateRock(int x, int y)
    {
        GameObject rock = Instantiate(rockPrefab) as GameObject;
        rock.transform.SetParent(mainPanel.transform, false);
        rock.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        rock.gameObject.SetActive(true);
        rocks.Add(rock);

        LeanTween.scale(rock.GetComponent<RectTransform>(), rock.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateDiamond(int x, int y)
    {
        GameObject diamond = Instantiate(diamondPrefab) as GameObject;
        diamond.transform.SetParent(mainPanel.transform, false);
        diamond.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        diamond.gameObject.SetActive(true);
        rocks.Add(diamond);

        LeanTween.scale(diamond.GetComponent<RectTransform>(), diamond.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateGroundTile(int x, int y)
    {
        GameObject groundTile = Instantiate(groundTilePrefab) as GameObject;
        groundTile.transform.SetParent(mainPanel.transform, false);
        groundTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        groundTile.gameObject.SetActive(true);
        rocks.Add(groundTile);

        LeanTween.scale(groundTile.GetComponent<RectTransform>(), groundTile.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateCable(int x, int y)
    {
        GameObject cable = Instantiate(cablePrefab) as GameObject;
        cable.transform.SetParent(mainPanel.transform, false);
        cable.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        cable.gameObject.SetActive(true);
        rocks.Add(cable);

        LeanTween.scale(cable.GetComponent<RectTransform>(), cable.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateBomb(int x, int y)
    {
        GameObject bomb = Instantiate(cablePrefab) as GameObject;
        bomb.transform.SetParent(mainPanel.transform, false);
        bomb.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        bomb.gameObject.SetActive(true);
        rocks.Add(bomb);

        LeanTween.scale(bomb.GetComponent<RectTransform>(), bomb.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void checkDrillerStuck()
    {
       if(stuckTimer <= 0)
       {
           succeededDrill = false;
           state = DrillingGameState.STARTSTOPTOAST;
           stuckTimer = stuckTime;
       }
    }
}
