using UnityEngine;
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
    [SerializeField] private UnityEngine.UI.Image joystickImage;
    [SerializeField] private Sprite arrowDown;
    [SerializeField] private Sprite arrowUpDown;
    [SerializeField] private Sprite arrowLeftRight;
    [SerializeField] public int[] columns;
    [SerializeField] public int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private GameObject waterTilePrefab;
    [SerializeField] private GameObject groundYellow;
    [SerializeField] private GameObject groundGreen;
    [SerializeField] private GameObject groundOrange;
    [SerializeField] private GameObject groundRed;
    [SerializeField] private GameObject groundAcid;
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
    [SerializeField] private TextAsset[] easyLevels;
    [SerializeField] private TextAsset[] mediumLevels;
    [SerializeField] private TextAsset[] hardLevels;

    private Rigidbody2D myBody;
    private Drillspot drillspot;
    private GameObject ceiling;
    private GameObject rightWall;
    private GameObject leftWall;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    public enum DrillingDirection {  UP, DOWN, LEFT, RIGHT, NONE }
    private DrillingGameState state;
    private DrillingDirection drillDir;
    private DrillingDirection prevDrillDir;
    private Vector2 initDrillPos;
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int tileTweenId;
    //bools
    private bool slidingLeft, makeDrill, imagesActivated = false;
    //timers
    private float toastTimer;
    private float jumpPhaseTimer;
    private float panelSlidingTimer;
    private float drillStuckChecked;
    private float stuckTimer;

    public bool SucceededDrill { get; set; }
    public bool Bumped { get; set; }
    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> water = new List<GameObject>();
    private List<GameObject> UIwater = new List<GameObject>();
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection DrillDirection { get { return drillDir; } set { drillDir = value; } }
    public DrillingDirection PrevDrillDirection { get { return prevDrillDir; } set { prevDrillDir = value; } }
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
        drillStuckChecked = Time.time;
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height) - 700, 0);
        if (drill) initDrillPos = drill.rectTransform.anchoredPosition;
        myBody = drill.GetComponent<Rigidbody2D>();
        ceiling = GameObject.Find("Ceiling");
        rightWall = GameObject.Find("Right wall");
        leftWall = GameObject.Find("Left wall");
        SucceededDrill = true;
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        imagesActivated = true;
        drill.transform.SetAsLastSibling();
        if (animator) animator.SetBool("isSlidingLeft", false);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        stuckTimer = stuckTime;
        myBody.inertia = 0;
        prevDrillDir = DrillingDirection.NONE;
        drillDir = DrillingDirection.NONE;
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
            if (!AutoWin) state = DrillingGameState.SLIDING;
            else state = DrillingGameState.SUCCESS;

            if (levelsCounter < 3)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(easyLevels[(SucceededDrill) ? levelsCounter : Random.Range(0, 3)]);
                generateLevel(tiles);  // pre-designed levels, loading from csv
            }
            else if (levelsCounter >=3 && levelsCounter < 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(mediumLevels[(SucceededDrill) ? levelsCounter - 3 : Random.Range(3, 6)]);
                generateLevel(tiles);  // pre-designed levels, loading from csv
            }
            else if (levelsCounter >= 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(hardLevels[(SucceededDrill) ? levelsCounter - 6 : Random.Range(6, 9)]);
                generateLevel(tiles); // pre-designed levels, loading from csv
            }
            if (SucceededDrill) levelsCounter++;

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
        if (!ReachedBottom(616, drill)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void drillDown()
    {
        switch(prevDrillDir)
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
                        targetColumn++;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    prevDrillDir = DrillingDirection.NONE;
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
                        targetColumn--;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    prevDrillDir = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
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

        if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y <= rows[targetRow + 1]) targetRow++;
    }

    private void drillLeft()
    {
        switch (prevDrillDir)
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
                        targetRow++;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    prevDrillDir = DrillingDirection.NONE;
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
                        targetRow--;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    prevDrillDir = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
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

        if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x <= columns[targetColumn-1]) targetColumn -= 1;
    }

    private void drillRight()
    {
        switch (prevDrillDir)
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
                        targetRow++;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    prevDrillDir = DrillingDirection.NONE;
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
                        targetRow--;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    prevDrillDir = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
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

        if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x >= columns[targetColumn+1]) targetColumn += 1;
    }

    private void drillUp()
    {
        switch (prevDrillDir)
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
                        targetColumn++;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    prevDrillDir = DrillingDirection.NONE;
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
                        targetColumn--;
                        prevDrillDir = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    prevDrillDir = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
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

        if (targetRow > 0 && drill.rectTransform.anchoredPosition.y >= rows[targetRow-1]) targetRow--;
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
        }
    }

    private void handleSlidingState()
    {
        activateImages(true);
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
            //Debug.Log("row: " + targetRow + " | column: " + targetColumn + " | prev dir: " + prevDrillDir + " | dir: " + drillDir + " | bump: " + Bumped +
            //    " | stuck: " + stuckTimer);
        }
        updateProgressBars();
        updateJoystickImages();
        updateWallsEnabling();

        if (state == DrillingGameState.DRILLING)
        {
            if (Input.GetKeyDown(KeyCode.N)) drillSpeed++;
            if (Input.GetKeyDown(KeyCode.M)) drillSpeed--;
        }
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
        SucceededDrill = false;
        targetColumn = -1;
        targetRow = -1;
        foreach (GameObject rock in tiles) Destroy(rock);
        foreach (GameObject drop in water) Destroy(drop);
        drill.rectTransform.anchoredPosition = initDrillPos;
        tiles.Clear();
        water.Clear();
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

    private void updateJoystickImages()
    {
        if(state == DrillingGameState.SLIDING)
            joystickImage.sprite = arrowDown;
        if (state == DrillingGameState.DRILLING)
        {
            if(drillDir == DrillingDirection.UP || drillDir == DrillingDirection.DOWN)
                joystickImage.sprite = arrowLeftRight;
            else if(drillDir == DrillingDirection.LEFT || drillDir == DrillingDirection.RIGHT)
                joystickImage.sprite = arrowUpDown;
        }
        else joystickImage.sprite = arrowDown;
    }

    private void updateWallsEnabling()
    {
        if (drill.rectTransform.anchoredPosition.y <= rows[0])
        {
            Debug.Log("its true!");
            if (!ceiling.GetComponent<BoxCollider2D>().enabled) ceiling.GetComponent<BoxCollider2D>().enabled = true;
            if (!rightWall.GetComponent<BoxCollider2D>().enabled) rightWall.GetComponent<BoxCollider2D>().enabled = true;
            if (!leftWall.GetComponent<BoxCollider2D>().enabled) leftWall.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}
