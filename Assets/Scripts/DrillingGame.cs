using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DrillingDirection { UP, DOWN, LEFT, RIGHT, NONE }

public class DrillingGame : Minigame
{
    //temp - jacques test
    private DrillGameMap map;
    [SerializeField] private RectTransform mapPanel;

    [SerializeField] private float toastMessageTime = 3.0f;
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float succeededDrillValue = 5.0f;
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] public float stuckTime = 10.0f;
    [SerializeField] private float drillStuckCooldown = 2.0f;
    [SerializeField] private float flashTileTime = 1.0f;
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image endFailToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;
    [SerializeField] private UnityEngine.UI.Image flashTile;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private GameObject waterTilePrefab;
    [SerializeField] private GameObject groundYellow;
    [SerializeField] private GameObject groundGreen;
    [SerializeField] private GameObject groundOrange;
    [SerializeField] private GameObject groundRed;
    [SerializeField] private GameObject groundAcid;
    [SerializeField] private GameObject pipeVertical;
    [SerializeField] private GameObject pipeHorizontal;
    [SerializeField] private GameObject pipeCurve1;
    [SerializeField] private GameObject pipeCurve2;
    [SerializeField] private GameObject pipeCurve3;
    [SerializeField] private GameObject pipeCurve4;
    [SerializeField] private bool AutoWin;
    [SerializeField] private Animator animator;
    [SerializeField] public int[] columns;
    [SerializeField] public int[] rows;
    [SerializeField] private TextAsset[] easyLevels;
    [SerializeField] private TextAsset[] mediumLevels;
    [SerializeField] private TextAsset[] hardLevels;

    private Rigidbody2D myBody;
    private Drillspot drillspot;
    private GameObject ceiling;
    private GameObject rightWall;
    private GameObject leftWall;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    private DrillingGameState state;
    private Vector2 initDrillPos;
    private Vector2 flashCoords;
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int tileTweenId;
    private int curveId = 0;
    //bools
    private bool slidingLeft, makeDrill, imagesActivated, joystickShaken, triggerFlash = false;
    //timers
    private float toastTimer;
    private float jumpPhaseTimer;
    private float panelSlidingTimer;
    private float drillStuckChecked;
    private float stuckTimer;
    private float flashTileTimer;

    public bool SucceededDrill { get; set; }
    public bool Bumped { get; set; }
    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> water = new List<GameObject>();
    private List<GameObject> UIwater = new List<GameObject>();
    private List<GameObject> bottomRow = new List<GameObject>();

    public DrillingAgent DrillingAgent { get; private set; }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public void MakeDrill(bool value) { makeDrill = value; }
    public float StuckTimer { get { return stuckTimer; } set { stuckTimer = value; } }
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

    void Awake()
    {
        //jacques map refactor test

//        DrillingAgent = FindObjectOfType<DrillingAgent>();
//        Vector2 tileSize = new Vector2(44, 44);
//        Vector2 mapDimmensions = new Vector2(19, 14);
//
//        map = GetComponent<DrillGameMap>();
//        map.Initialize(mapPanel, mapDimmensions, tileSize, GameManager.Instance.LoadDrillingPuzzle(easyLevels[0]));
//        DrillingAgent.Map = map;
//        DrillingAgent.SetGridPosition(0, 8);

        //jacques map refactor test

        /*if(rockPrefab)
            tileTweenId = LeanTween.scale(rockPrefab.GetComponent<RectTransform>(), rockPrefab.GetComponent<RectTransform>().localScale * 1.2f, 1f)
                .setEase(LeanTweenType.punch).id;*/
    }

    void Start()
    {
        activateImages(false);
        targetColumn = -1;
        targetRow = -1;
        toastTimer = toastMessageTime;
        jumpPhaseTimer = jumpPhaseTime;
        panelSlidingTimer = panelSlidingTime;
        flashTileTimer = flashTileTime;
        drillStuckChecked = Time.time;
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height) - 700, 0);
        if (drill) initDrillPos = drill.rectTransform.anchoredPosition;
        myBody = drill.GetComponent<Rigidbody2D>();
        ceiling = GameObject.Find("Ceiling");
        rightWall = GameObject.Find("Right wall");
        leftWall = GameObject.Find("Left wall");
        SucceededDrill = true;
        levelsCounter = 0;
        GameManager.Instance.Joystick.JoystickPanel.transform.SetParent(mainPanel.transform, true);
        GameManager.Instance.Joystick.JoystickPanel.GetComponent<UnityEngine.UI.Image>().rectTransform.anchoredPosition = new Vector2(500, -100);
        GameManager.Instance.Joystick.JoystickPanel.transform.localScale = new Vector3(1, 1, 1);
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        imagesActivated = true;
        drill.transform.SetAsLastSibling();
        if (animator) animator.SetBool("isSlidingLeft", false);
        GameManager.Instance.Joystick.InnerPad.GetComponent<UnityEngine.UI.Image>().rectTransform.anchoredPosition = new Vector2(0, 0);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        stuckTimer = stuckTime;
        myBody.inertia = 0;
    }

    // 0 - diamond, 1 - electricity, 2 - yellow, 3 - blocks, 4 - green, 5 - orange, 6 - red, 7 - water, 8 - yellow-egg
    private void generateLevel(int[] tiles)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < columns.Length; j++)
            {
                int id = tiles[(columns.Length * i) + j];
                switch(id)
                {
                    case 0:
                        instantiateDiamond(columns[j], rows[i]);
                        break;
                    case 1:
                        instantiateCable(columns[j], rows[i]);
                        break;
                    case 2: case 4: case 5: case 6: case 8:
                        instantiateGroundTile(columns[j], rows[i], id);
                        break;
                    case 3:
                        instantiateRock(columns[j], rows[i]);
                        break;
                    case 7:
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
            if (levelsCounter < 3)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(easyLevels[(SucceededDrill == true) ? levelsCounter : Random.Range(0, 3)]);
                generateLevel(tiles);  // pre-designed levels, loading from csv
            }
            else if (levelsCounter >=3 && levelsCounter < 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(mediumLevels[(SucceededDrill == true) ? levelsCounter - 3 : Random.Range(3, 6)]);
                generateLevel(tiles);  // pre-designed levels, loading from csv
            }
            else if (levelsCounter >= 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(hardLevels[(SucceededDrill == true) ? levelsCounter - 6 : Random.Range(6, 9)]);
                generateLevel(tiles); // pre-designed levels, loading from csv
            }
            if (SucceededDrill) levelsCounter++;
            panelSlidingTimer = panelSlidingTime;
            GameManager.Instance.Joystick.StartPosition = GameManager.Instance.Joystick.transform.position;
            if (!AutoWin) state = DrillingGameState.SLIDING;
            else state = DrillingGameState.SUCCESS;
        }
        
    }

    private void handlePreDrillJump()
    {
        jumpPhaseTimer -= Time.deltaTime;
        if (jumpPhaseTimer <= 0)
        {
            animator.SetBool("isDrillingDown", true);
            state = DrillingGameState.DRILLING;
            jumpPhaseTimer = jumpPhaseTime;
        }
    }

    private void handleStartStopState()
    {
        toastTimer -= Time.deltaTime;
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
            if (toastTimer < 0.0f) fireToast(false);
        }
    }

    private void fireToast(bool gameSucceeded)
    {
        toastTimer = toastMessageTime;
        if (gameSucceeded)
        {
            endOkToast.gameObject.SetActive(false);
            LeanTween.move(steamImage.gameObject.GetComponent<RectTransform>(), new Vector3(0, -475, 0), toastMessageTime).setEase(LeanTweenType.easeOutQuad);
        }
        else
        {
            endFailToast.gameObject.SetActive(false);
            drillLife.fillAmount = 0f;
        }
        state = DrillingGameState.INACTIVE;

        End(gameSucceeded);
    }

    private void handleDrillingState()
    {
        if (!ReachedBottom(636, drill)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void drillDown()
    {
        switch(GameManager.Instance.Joystick.PrevInput)
        {
            case DrillingDirection.RIGHT:
                if (targetColumn < columns.Length - 1 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1])
                    {
                        myBody.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeHorizontal();
                        curveId = 1;
                        targetColumn++;
                        animator.SetBool("isDrillingRight", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 1;
                    animator.SetBool("isDrillingRight", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.LEFT:
                if (targetColumn > 0 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                    {
                        myBody.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeHorizontal();
                        curveId = 3;
                        targetColumn--;
                        animator.SetBool("isDrillingLeft", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 3;
                    animator.SetBool("isDrillingLeft", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y <= rows[targetRow + 1])
                {
                    if(GameManager.Instance.Joystick.JustMoved)
                    {
                        instantiatePipeCurve(curveId);
                        GameManager.Instance.Joystick.JustMoved = false;
                    }
                    else instantiatePipeVertical();
                    targetRow++;
                }
                if (!animator.GetBool("isDrillingDown")) animator.SetBool("isDrillingDown", true);
                myBody.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                myBody.freezeRotation = true;
                break;
            default:
                myBody.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                myBody.freezeRotation = true;
                break;
        }
    }

    private void drillLeft()
    {
        switch (GameManager.Instance.Joystick.PrevInput)
        {
            case DrillingDirection.DOWN:
                if (targetRow < rows.Length - 1 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.y > rows[targetRow + 1])
                    {
                        myBody.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeVertical();
                        curveId = 4;
                        targetRow++;
                        animator.SetBool("isDrillingDown", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != rows[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 4;
                    animator.SetBool("isDrillingDown", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.UP:
                if (targetRow > 0 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.y < rows[targetRow - 1])
                    {
                        myBody.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeVertical();
                        curveId = 1;
                        targetRow--;
                        animator.SetBool("isDrillingUp", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 1;
                    animator.SetBool("isDrillingUp", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1])
                {
                    if (GameManager.Instance.Joystick.JustMoved)
                    {
                        instantiatePipeCurve(curveId);
                        GameManager.Instance.Joystick.JustMoved = false;
                    }
                    else instantiatePipeHorizontal();
                    targetColumn -= 1;
                }
                if (!animator.GetBool("isDrillingLeft")) animator.SetBool("isDrillingLeft", true);
                myBody.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                myBody.freezeRotation = true;
                break;
            default:
                myBody.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                myBody.freezeRotation = true;
                break;
        }
    }

    private void drillRight()
    {
        switch (GameManager.Instance.Joystick.PrevInput)
        {
            case DrillingDirection.DOWN:
                if (targetRow < rows.Length - 1 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.y > rows[targetRow + 1])
                    {
                        myBody.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeVertical();
                        curveId = 2;
                        flashCoords = new Vector2(columns[targetColumn + 1], rows[targetRow + 1]);
                        triggerFlash = true;
                        targetRow++;
                        animator.SetBool("isDrillingDown", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != rows[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 2;
                    animator.SetBool("isDrillingDown", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.UP:
                if (targetRow > 0 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.y < rows[targetRow - 1])
                    {
                        myBody.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeVertical();
                        curveId = 3;
                        targetRow--;
                        animator.SetBool("isDrillingUp", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 3;
                    animator.SetBool("isDrillingUp", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn < columns.Length - 1 && drill.rectTransform.anchoredPosition.x >= columns[targetColumn + 1])
                {
                    if (GameManager.Instance.Joystick.JustMoved)
                    {
                        instantiatePipeCurve(curveId);
                        GameManager.Instance.Joystick.JustMoved = false;
                    }
                    else instantiatePipeHorizontal();
                    targetColumn += 1;
                }
                if (!animator.GetBool("isDrillingRight")) animator.SetBool("isDrillingRight", true);
                myBody.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                myBody.freezeRotation = true;
                break;
            default:
                myBody.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                myBody.freezeRotation = true;
                break;
        }
    }

    private void drillUp()
    {
        switch (GameManager.Instance.Joystick.PrevInput)
        {
            case DrillingDirection.RIGHT:
                if (targetColumn < columns.Length - 1 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1])
                    {
                        myBody.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeHorizontal();
                        curveId = 4;
                        targetColumn++;
                        animator.SetBool("isDrillingRight", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 4;
                    animator.SetBool("isDrillingRight", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.LEFT:
                if (targetColumn > 0 && !Bumped)
                {
                    if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                    {
                        myBody.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                        myBody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        myBody.freezeRotation = true;
                    }
                    else
                    {
                        instantiatePipeHorizontal();
                        curveId = 2;
                        targetColumn--;
                        animator.SetBool("isDrillingLeft", false);
                        GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 2;
                    animator.SetBool("isDrillingLeft", false);
                    GameManager.Instance.Joystick.PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow > 0 && drill.rectTransform.anchoredPosition.y >= rows[targetRow - 1])
                {
                    if (GameManager.Instance.Joystick.JustMoved)
                    {
                        instantiatePipeCurve(curveId);
                        GameManager.Instance.Joystick.JustMoved = false;
                    }
                    else instantiatePipeVertical();
                    targetRow--;
                }
                if (!animator.GetBool("isDrillingUp")) animator.SetBool("isDrillingUp", true);
                myBody.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                myBody.freezeRotation = true;
                break;
            default:
                myBody.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                myBody.constraints = RigidbodyConstraints2D.FreezePositionX;
                myBody.freezeRotation = true;
                break;
        }
    }
    
    private void updateDrilling()
    {
        switch (GameManager.Instance.Joystick.CurrentInput)
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
        }
    }

    private void handleSlidingState()
    {
        activateImages(true);
        if (!joystickShaken)
        {
            LeanTween.scale(GameManager.Instance.Joystick.JoystickPanel.GetComponent<RectTransform>(),
                GameManager.Instance.Joystick.JoystickPanel.GetComponent<RectTransform>().localScale * 1.2f, 3)
                .setEase(LeanTweenType.punch);
            joystickShaken = true;
        }
        updateSlidingMovement();
    }

    //columns and rows increment/decrement tested and works fine here
    private void updateSlidingMovement()
    {
        if (!makeDrill)
        {
            if (slidingLeft == false)
            {
                if (targetColumn < columns.Length-1)
                {
                    if (drill.rectTransform.anchoredPosition.x >= columns[targetColumn+1]) targetColumn += 1;
                }
                else
                {
                    slidingLeft = true;
                    animator.SetBool("isSlidingLeft", true);
                }
                drill.transform.Translate(new Vector3(1 * slideSpeed * Time.deltaTime, 0, 0));
            }
            else
            {
                if (targetColumn > 0)
                {
                    if (drill.rectTransform.anchoredPosition.x <= columns[targetColumn-1]) targetColumn -= 1;
                }
                else
                {
                    slidingLeft = false;
                    animator.SetBool("isSlidingLeft", false);
                }
                drill.transform.Translate(new Vector3(-1 * slideSpeed * Time.deltaTime, 0, 0));
            }
        }
        else
        {
            if (slidingLeft == false)
            {
                if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x < columns[targetColumn+1])
                    drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn+1], drill.rectTransform.anchoredPosition.y);
                else
                {
                    state = DrillingGameState.PREDRILLJUMP;
                    if (targetColumn < columns.Length) targetColumn += 1;
                }
            }
            else
            {
                if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x > columns[targetColumn-1])
                    drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn-1], drill.rectTransform.anchoredPosition.y);
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
        }
        updateProgressBars();
        updateWallsEnabling();
        if (triggerFlash) FlashTile();
        //Debug.Log("cur: " + GameManager.Instance.Joystick.CurrentInput + " | prev: " + GameManager.Instance.Joystick.PrevInput);
    }

    private void updateProgressBars()
    {
        if(waterBar && water.Count <= 3) waterBar.fillAmount = water.Count * 33.33333334f / 100f;
        if (stuckTimer > stuckTime - (stuckTime / 3)) drillLife.fillAmount = 1.00f;
        else if (stuckTimer <= stuckTime - (stuckTime / 3) && stuckTimer > stuckTime - (stuckTime / 3)*2) drillLife.fillAmount = 0.66f;
        else if (stuckTimer <= stuckTime - (stuckTime / 3) * 2 && stuckTimer > 0.05f) drillLife.fillAmount = 0.33f;
        else drillLife.fillAmount = 0.00f;
        if (water.Count == 3) foreach (GameObject rock in bottomRow) Destroy(rock);
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
        joystickShaken = false;
        GameManager.Instance.Joystick.JustMoved = false;
        animator.SetBool("isSlidingLeft", false);
        animator.SetBool("isDrillingDown", false);
        animator.SetBool("isDrillingUp", false);
        animator.SetBool("isDrillingRight", false);
        animator.SetBool("isDrillingLeft", false);
        animator.SetBool("shouldJump", false);
        SucceededDrill = false;
        targetColumn = -1;
        targetRow = -1;
        foreach (GameObject rock in tiles) Destroy(rock);
        foreach (GameObject drop in water) Destroy(drop);
        foreach (GameObject drop in UIwater) Destroy(drop);
        foreach (GameObject rock in bottomRow) Destroy(rock);
        drill.rectTransform.anchoredPosition = initDrillPos;
        tiles.Clear();
        water.Clear();
        UIwater.Clear();
        bottomRow.Clear();
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        drill.color = new Color(1, 1, 1);
        drillLife.color = new Color(1, 1, 1);
        waterBar.fillAmount = 0f;
        drillLife.fillAmount = 1f;
        if (ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = false;
        if (rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = false;
        if (leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void instantiateRock(int x, int y)
    {
        GameObject rock = Instantiate(rockPrefab) as GameObject;
        rock.transform.SetParent(mainPanel.transform, false);
        rock.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        rock.gameObject.SetActive(true);
        tiles.Add(rock);
        if (y == rows[rows.Length - 1]) bottomRow.Add(rock);
    }

    private void instantiateDiamond(int x, int y)
    {
        GameObject diamond = Instantiate(diamondPrefab) as GameObject;
        diamond.transform.SetParent(mainPanel.transform, false);
        diamond.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        diamond.gameObject.SetActive(true);
        tiles.Add(diamond);
    }

    private void instantiateGroundTile(int x, int y, int type)
    {
        GameObject groundTile;
        switch(type)
        {
            case 2:
                groundTile = Instantiate(groundYellow) as GameObject;
                break;
            case 4:
                groundTile = Instantiate(groundGreen) as GameObject;
                break;
            case 5:
                groundTile = Instantiate(groundOrange) as GameObject;
                break;
            case 6:
                groundTile = Instantiate(groundRed) as GameObject;
                break;
            case 8:
                groundTile = Instantiate(groundAcid) as GameObject;
                break;
            default:
                groundTile = Instantiate(groundYellow) as GameObject;
                break;
        }
        groundTile.transform.SetParent(mainPanel.transform, false);
        groundTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        groundTile.gameObject.SetActive(true);
        tiles.Add(groundTile);
    }

    private void instantiatePipeVertical()
    {
        if (targetRow >= 0 && targetColumn >= 0)
        {
            GameObject pipeVert = Instantiate(pipeVertical) as GameObject;
            pipeVert.transform.SetParent(mainPanel.transform, false);
            pipeVert.GetComponent<RectTransform>().anchoredPosition = new Vector2(columns[targetColumn], rows[targetRow]);
            pipeVert.gameObject.SetActive(true);
            tiles.Add(pipeVert);
        }
    }

    private void instantiatePipeHorizontal()
    {
        if (targetRow >= 0 && targetColumn >= 0)
        {
            GameObject pipeHor = Instantiate(pipeHorizontal) as GameObject;
            pipeHor.transform.SetParent(mainPanel.transform, false);
            pipeHor.GetComponent<RectTransform>().anchoredPosition = new Vector2(columns[targetColumn], rows[targetRow]);
            pipeHor.gameObject.SetActive(true);
            tiles.Add(pipeHor);
        }
    }

    private void instantiatePipeCurve(int id)
    {
        if (targetRow >= 0 && targetColumn >= 0)
        {
            GameObject pipeCurve;
            switch(id)
            {
                case 1:
                    pipeCurve = Instantiate(pipeCurve1) as GameObject;
                    break;
                case 2:
                    pipeCurve = Instantiate(pipeCurve2) as GameObject;
                    break;
                case 3:
                    pipeCurve = Instantiate(pipeCurve3) as GameObject;
                    break;
                case 4:
                    pipeCurve = Instantiate(pipeCurve4) as GameObject;
                    break;
                default:
                    pipeCurve = Instantiate(pipeCurve1) as GameObject;
                    break;
            }
            pipeCurve.transform.SetParent(mainPanel.transform, false);
            pipeCurve.GetComponent<RectTransform>().anchoredPosition = new Vector2(columns[targetColumn], rows[targetRow]);
            pipeCurve.gameObject.SetActive(true);
            tiles.Add(pipeCurve);
        }
    }

    private void instantiateCable(int x, int y)
    {
        GameObject cable = Instantiate(cablePrefab) as GameObject;
        cable.transform.SetParent(mainPanel.transform, false);
        cable.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        cable.gameObject.SetActive(true);
        tiles.Add(cable);
    }

    private void instantiateWaterTile(int x, int y)
    {
        GameObject waterTile = Instantiate(waterTilePrefab) as GameObject;
        waterTile.transform.SetParent(mainPanel.transform, false);
        waterTile.gameObject.SetActive(true);
        tiles.Add(waterTile);
        UIwater.Add(waterTile);

        if(UIwater.Count == 1)
        {
            waterTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 583);
            LeanTween.move(waterTile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.scale(waterTile.GetComponent<RectTransform>(), waterTile.GetComponent<RectTransform>().localScale * 0.5f, 1f);
        }
        else if (UIwater.Count == 2)
        {
            waterTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 497);
            LeanTween.move(waterTile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.scale(waterTile.GetComponent<RectTransform>(), waterTile.GetComponent<RectTransform>().localScale * 0.5f, 1f);
        }
        else if (UIwater.Count == 3)
        {
            waterTile.GetComponent<RectTransform>().anchoredPosition = new Vector2(-38, 409);
            LeanTween.move(waterTile.gameObject.GetComponent<RectTransform>(), new Vector2(x, y), 1.5f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.scale(waterTile.GetComponent<RectTransform>(), waterTile.GetComponent<RectTransform>().localScale * 0.5f, 1f);
        }
    }

    public void AddWater(GameObject waterPiece)
    {
        this.water.Add(waterPiece);
    }

    private void updateWallsEnabling()
    {
        if (drill.rectTransform.anchoredPosition.y <= rows[0])
        {
            if (!ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = true;
            if (!rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = true;
            if (!leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void FlashTile()
    {
        flashTileTimer -= Time.deltaTime;
        flashTile.rectTransform.anchoredPosition = flashCoords;
        flashTile.transform.SetAsLastSibling();
        flashTile.enabled = true;
        if (flashTileTimer <= 0)
        {
            flashTileTimer = flashTileTime;
            flashTile.transform.SetAsFirstSibling();
            flashTile.enabled = false;
            triggerFlash = false;
        }
    }
}
