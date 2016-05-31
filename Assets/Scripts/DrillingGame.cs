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
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image endFailToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;
    [SerializeField] public int[] columns;
    [SerializeField] public int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private GameObject waterTilePrefab;
    [SerializeField] private bool AutoWin;
    [SerializeField] private float toastMessageTime = 3.0f;
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float succeededDrillValue = 5.0f;
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] public float stuckTime = 10.0f;
    [SerializeField] private float drillStuckCooldown = 2.0f;
    [SerializeField] private Animator animator;
    [SerializeField] private MobileJoystick joystick;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    public enum DrillingDirection {  UP, DOWN, LEFT, RIGHT, IDLE }
    private DrillingGameState state;
    private DrillingDirection drillDir;
    private DrillingDirection prevDrillDir;
    private Vector3 initDrillPos;
    private Vector3 drillPrevPosition;
    private int targetColumn;
    private int targetRow;
    //bools
    private bool introShown, finalShown, slidingLeft, makeDrill, imagesActivated = false;
    //timers
    private float toastTimer;
    private float jumpPhaseTimer;
    private float panelSlidingTimer;
    private float drillStuckChecked;
    private float stuckTimer;

    // 19 X 14 test level tiles: ids to instantiate different objects
    private int[] levelTiles = 
    {
        1,1,4,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        1,5,4,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,3,1,1,1,1,1,
        1,1,4,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,3,1,
        1,5,4,1,1,1,1,1,1,1,3,3,2,3,3,3,3,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,3,1,1,1,1,1,
        1,1,4,1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,5,1,
        1,5,4,1,1,5,1,1,1,1,1,1,1,1,1,1,3,1,1,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        3,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3
    };

    public bool SucceededDrill { get; set; }
    public bool Bumped { get; set; }
    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> water = new List<GameObject>();
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection DrillDirection { get { return drillDir; } set { drillDir = value; } }
    public DrillingDirection PrevDrillDirection { get { return prevDrillDir; } set { prevDrillDir = value; } }
    public void MakeDrill(bool value) { makeDrill = value; }
    public float StuckTimer { get { return stuckTimer; } set { stuckTimer = value; } }
    public Vector3 DrillPrevPosition { get { return drillPrevPosition; } set { drillPrevPosition = value; } }
    public UnityEngine.UI.Image Drill { get { return drill; } }
    public UnityEngine.UI.Image WaterBar { get { return waterBar; } }
    public UnityEngine.UI.Image DrillLife { get { return drillLife; } }
    public float DiamondValue { get { return diamondValue; } }
    public UnityEngine.UI.Image MainPanel { get { return mainPanel; } }
    public Animator Animator { get { return animator; } }
    public int GetWaterCount { get { return water.Count; } }
    public int TargetRow { get { return targetRow; } }
    public int TargetColumn { get { return targetColumn; } }
    public bool ReachedBottom(int bottom, UnityEngine.UI.Image drill)
    {
        return drill.rectTransform.anchoredPosition.y <= initDrillPos.y - bottom;
    }

    void Start()
    {
        activateImages(false);
        targetColumn = 0;
        targetRow = 0;
        toastTimer = toastMessageTime;
        jumpPhaseTimer = jumpPhaseTime;
        panelSlidingTimer = panelSlidingTime;
        drillStuckChecked = Time.time;
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height) - 700, 0);
        if (drill) initDrillPos = drill.rectTransform.anchoredPosition;
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        imagesActivated = true;
        introShown = true;
        drill.transform.SetAsLastSibling();
        if (animator) animator.SetBool("isSlidingLeft", false);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        stuckTimer = stuckTime;
    }

    private void generateProceduralMap()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < rows.Length-1; j++)
            {
                if (j == 0) instantiateGroundTile(columns[i], rows[j]);
                else
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand <= 0.05f) instantiateCable(columns[i], rows[j]); //try cable
                    else
                    {
                        rand = Random.Range(0f, 1f);
                        if (rand <= 0.08f) instantiateRock(columns[i], rows[j]); //try rock
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

    // 0 - diamond, 1 - electricity, 3 - yellow, 4 - blocks, 5 - green, 6 - orange, 7 - red, 8 - water, 9 - yellow-egg
    private void generateLevel(int[] tiles)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < columns.Length; j++)
            {
                int id = tiles[(columns.Length * i) + j];
                switch(id)
                {
                    case 1:
                        instantiateGroundTile(columns[j], rows[i]);
                        break;
                    case 2:
                        instantiateCable(columns[j], rows[i]);
                        break;
                    case 3:
                        instantiateRock(columns[j], rows[i]);
                        break;
                    case 4:
                        instantiateDiamond(columns[j], rows[i]);
                        break;
                    case 5:
                        instantiateWaterTile(columns[j], rows[i]);
                        break;
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
            case DrillingGameState.PREDRILLJUMP:
                handlePreDrillJump();
                break;
            case DrillingGameState.ACTIVATION:
                handleActivation();
                break;
        }
    }

    private void handleActivation()
    {
        panelSlidingTimer -= Time.deltaTime;
        if(panelSlidingTimer <= 0)
        {
            if (!AutoWin) state = DrillingGameState.SLIDING;
            else state = DrillingGameState.SUCCESS;
            //generateProceduralMap(); // proceduraly generated level, spawning percentage share is based on inspector values or curves
            generateLevel(levelTiles); // pre-designed levels, loading from csv
            panelSlidingTimer = panelSlidingTime;
            joystick.StartPosition = joystick.transform.position;
        }
        
    }

    private void handlePreDrillJump()
    {
        jumpPhaseTimer -= Time.deltaTime;
        if (jumpPhaseTimer <= 0)
        {
            animator.SetBool("isDrilling", true);
            state = DrillingGameState.DRILLING;
            drillDir = DrillingDirection.DOWN;
            jumpPhaseTimer = jumpPhaseTime;
        }
    }

    private void handleStartStopState()
    {
        toastTimer -= Time.deltaTime;
        if(introShown && !finalShown)
        {
            if (SucceededDrill)
            {
                endOkToast.gameObject.SetActive(true);
                endOkToast.gameObject.transform.parent.SetAsLastSibling();
                if (toastTimer > 0)
                    LeanTween.move(steamImage.gameObject.GetComponent<RectTransform>(), new Vector3(0, 50, 0), toastMessageTime).setEase(LeanTweenType.easeOutQuad);
                else fireToast(true);
            }
            else
            {
                endFailToast.gameObject.SetActive(true);
                endFailToast.gameObject.transform.parent.SetAsLastSibling();
                drillLife.fillAmount = 0f;
                if (toastTimer < 0.0f) fireToast(false);
            }
        }
    }

    private void fireToast(bool gameSucceeded)
    {
        finalShown = true;
        toastTimer = toastMessageTime;
        if(gameSucceeded) endOkToast.gameObject.SetActive(false);
        else endFailToast.gameObject.SetActive(false);
        state = DrillingGameState.INACTIVE;
        End(gameSucceeded);
    }

    private void handleDrillingState()
    {
        if (!ReachedBottom(616, drill)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void drillDown()
    {
        if (prevDrillDir == DrillingDirection.RIGHT)
        {
            if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x <= columns[targetColumn + 1])
                drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
            else
            {
                targetColumn++;
                prevDrillDir = DrillingDirection.DOWN;
            }
        }
        else if (prevDrillDir == DrillingDirection.LEFT)
        {
            if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x >= columns[targetColumn - 1])
                drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
            else
            {
                targetColumn--;
                prevDrillDir = DrillingDirection.DOWN;
            }
        }
        else drill.transform.Translate(0, -1 * drillSpeed * Time.deltaTime, 0); //drill down

        if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y < rows[targetRow + 1]) targetRow++;
    }

    private void drillLeft()
    {
        if (prevDrillDir == DrillingDirection.DOWN)
        {
            if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y >= rows[targetRow + 1])
                drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0); //drill down
            else
            {
                targetRow++;
                prevDrillDir = DrillingDirection.LEFT;
            }
        }
        else if (prevDrillDir == DrillingDirection.UP)
        {
            if (targetRow > 0 && drill.rectTransform.anchoredPosition.y <= rows[targetRow - 1])
                drill.transform.Translate(0, 1.0f * drillSpeed * Time.deltaTime, 0); //drill up
            else
            {
                targetRow--;
                prevDrillDir = DrillingDirection.LEFT;
            }
        }
        else
        {
            if (targetColumn > 0)
            {
                drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
                if (drill.rectTransform.anchoredPosition.x < columns[targetColumn - 1])
                {
                    targetColumn -= 1;
                    drill.color = new Color(1, 1, 1);
                    drillLife.color = new Color(1, 1, 1);
                    Bumped = false;
                }
            }
            else
            {
                stuckTimer -= Time.deltaTime;
                drill.color = new Color(1, 0, 0);
                drillLife.color = new Color(1, 0, 0);
                Bumped = true;
            }
        }
    }

    private void drillRight()
    {
        if (prevDrillDir == DrillingDirection.DOWN)
        {
            if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y >= rows[targetRow + 1])
                drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0); //drill down
            else
            {
                targetRow++;
                prevDrillDir = DrillingDirection.RIGHT;
            }
        }
        else if (prevDrillDir == DrillingDirection.UP)
        {
            if (targetRow > 0 && drill.rectTransform.anchoredPosition.y <= rows[targetRow - 1])
                drill.transform.Translate(0, 1.0f * drillSpeed * Time.deltaTime, 0); //drill up
            else
            {
                targetRow--;
                prevDrillDir = DrillingDirection.RIGHT;
            }
        }
        else
        {
            if (targetColumn < columns.Length - 1)
            {
                drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
                if (drill.rectTransform.anchoredPosition.x > columns[targetColumn + 1])
                {
                    targetColumn += 1;
                    drill.color = new Color(1, 1, 1);
                    drillLife.color = new Color(1, 1, 1);
                    Bumped = false;
                }
            }
            else
            {
                stuckTimer -= Time.deltaTime;
                drill.color = new Color(1, 0, 0);
                drillLife.color = new Color(1, 0, 0);
                Bumped = true;
            }
        }
    }

    private void drillUp()
    {
        if (prevDrillDir == DrillingDirection.RIGHT)
        {
            if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x <= columns[targetColumn + 1])
                drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
            else
            {
                targetColumn++;
                prevDrillDir = DrillingDirection.UP;
            }
        }
        else if (prevDrillDir == DrillingDirection.LEFT)
        {
            if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x >= columns[targetColumn - 1])
                drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
            else
            {
                targetColumn--;
                prevDrillDir = DrillingDirection.UP;
            }
        }
        else
        {
            if (targetRow > 0)
            {
                drill.transform.Translate(0, 1.0f * drillSpeed * Time.deltaTime, 0); //drill up
                if (drill.rectTransform.anchoredPosition.y > rows[targetRow - 1])
                {
                    targetRow--;
                    drill.color = new Color(1, 1, 1);
                    drillLife.color = new Color(1, 1, 1);
                    Bumped = false;
                }
            }
            else
            {
                stuckTimer -= Time.deltaTime;
                drill.color = new Color(1, 0, 0);
                drillLife.color = new Color(1, 0, 0);
                Bumped = true;
            }
        }
    }

    private void handleIdle()
    {

    }

    private void updateDrilling()
    {
        switch(drillDir)
        {
            case DrillingDirection.DOWN:
                drillDown();
                break;
            case DrillingDirection.LEFT:
                drillLeft();
                break;
            case DrillingDirection.RIGHT:
                drillRight();
                break;
            case DrillingDirection.UP:
                drillUp();
                break;
            case DrillingDirection.IDLE:
                handleIdle();
                break;
        }
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
                else
                {
                    slidingLeft = true;
                    animator.SetBool("isSlidingLeft", true);
                }
            }
            else
            {
                drill.transform.Translate(new Vector3(-1 * slideSpeed * Time.deltaTime, 0, 0));
                if (targetColumn > 0)
                {
                    if (drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1]) targetColumn -= 1;
                }
                else
                {
                    slidingLeft = false;
                    animator.SetBool("isSlidingLeft", false);
                }
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
                    state = DrillingGameState.PREDRILLJUMP;
                    if (targetColumn < columns.Length - 1) targetColumn += 1;
                }
            }
            else
            {
                if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                    drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn - 1], drill.rectTransform.anchoredPosition.y);
                else
                {
                    state = DrillingGameState.PREDRILLJUMP;
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
        SucceededDrill = true;
        state = DrillingGameState.STARTSTOPTOAST;
    }

    public override void Update()
    {
        base.Update();
        if (IsRunning && Timeleft <= 0.5f)
        {
            SucceededDrill = false;
            state = DrillingGameState.STARTSTOPTOAST;
        }
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && state != DrillingGameState.INACTIVE)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
        }
    }

    void FixedUpdate()
    {
        if(drill) updateState();
        if (Time.time - drillStuckChecked > drillStuckCooldown)
        {
            checkDrillerStuck();
            drillStuckChecked = Time.time;
            Debug.Log("row: " + targetRow + " | column: " + targetColumn);
        }
        drillPrevPosition = drill.rectTransform.anchoredPosition;
        updateProgressBars();
    }

    private void updateProgressBars()
    {
        if(waterBar && water.Count <= 3) waterBar.fillAmount = water.Count * 33.33333334f / 100f;
        if (stuckTimer > stuckTime - (stuckTime / 3)) drillLife.fillAmount = 1.00f;
        else if (stuckTimer <= stuckTime - (stuckTime / 3) && stuckTimer > stuckTime - (stuckTime / 3)*2) drillLife.fillAmount = 0.66f;
        else if (stuckTimer <= stuckTime - (stuckTime / 3) * 2 && stuckTimer > 0.05f) drillLife.fillAmount = 0.33f;
        else drillLife.fillAmount = 0.00f;

    }

    private void checkDrillerStuck()
    {
        if (stuckTimer <= 0)
        {
            SucceededDrill = false;
            state = DrillingGameState.STARTSTOPTOAST;
            stuckTimer = stuckTime;
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            plant.transform.SetParent(GameManager.Instance.PlanetTransform);
            GameManager.Instance.Player.StartBuildMinigame(plant, 1f);
            GameManager.Instance.Player.ScorePoints(succeededDrillValue);
        }
        else GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);

//        Destroy(drillspot.gameObject);
        resetGameGuts();
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (drill) drill.gameObject.SetActive(true);
        } else
        {
            if (drill) drill.gameObject.SetActive(false);
            if (endFailToast) endFailToast.gameObject.SetActive(false);
            if (endOkToast) endOkToast.gameObject.SetActive(false);
            imagesActivated = false;
        }
    }

    private void resetGameGuts()
    {
        makeDrill = false;
        slidingLeft = false;
        animator.SetBool("isSlidingLeft", false);
        animator.SetBool("isDrilling", false);
        animator.SetBool("shouldJump", false);
        introShown = false;
        finalShown = false;
        SucceededDrill = false;
        targetColumn = 0;
        targetRow = 0;
        foreach (GameObject rock in tiles) Destroy(rock);
        foreach (GameObject drop in water) Destroy(drop);
        drill.rectTransform.anchoredPosition = initDrillPos;
        tiles.Clear();
        water.Clear();
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        drill.color = new Color(1, 1, 1);
        drillLife.color = new Color(1, 1, 1);
        waterBar.fillAmount = 0f;
    }

    private void instantiateRock(int x, int y)
    {
        GameObject rock = Instantiate(rockPrefab) as GameObject;
        rock.transform.SetParent(mainPanel.transform, false);
        rock.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        rock.gameObject.SetActive(true);
        tiles.Add(rock);

        LeanTween.scale(rock.GetComponent<RectTransform>(), rock.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateDiamond(int x, int y)
    {
        GameObject diamond = Instantiate(diamondPrefab) as GameObject;
        diamond.transform.SetParent(mainPanel.transform, false);
        diamond.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        diamond.gameObject.SetActive(true);
        tiles.Add(diamond);

        LeanTween.scale(diamond.GetComponent<RectTransform>(), diamond.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateGroundTile(int x, int y)
    {
        GameObject groundTile = Instantiate(groundTilePrefab) as GameObject;
        groundTile.transform.SetParent(mainPanel.transform, false);
        groundTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        groundTile.gameObject.SetActive(true);
        tiles.Add(groundTile);

        LeanTween.scale(groundTile.GetComponent<RectTransform>(), groundTile.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateCable(int x, int y)
    {
        GameObject cable = Instantiate(cablePrefab) as GameObject;
        cable.transform.SetParent(mainPanel.transform, false);
        cable.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        cable.gameObject.SetActive(true);
        tiles.Add(cable);

        LeanTween.scale(cable.GetComponent<RectTransform>(), cable.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateWaterTile(int x, int y)
    {
        GameObject waterTile = Instantiate(waterTilePrefab) as GameObject;
        waterTile.transform.SetParent(mainPanel.transform, false);
        waterTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        waterTile.gameObject.SetActive(true);
        tiles.Add(waterTile);

        LeanTween.scale(waterTile.GetComponent<RectTransform>(), waterTile.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    public void AddWater(GameObject waterPiece)
    {
        this.water.Add(waterPiece);
    }
}
