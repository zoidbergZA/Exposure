using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DrillingDirection { UP, DOWN, LEFT, RIGHT, NONE }
public enum ToastType { SUCCESS, BROKEN_DRILL, BROKEN_PIPE, EXPLODED_BOMB, TRIGGERED_BOMB, NONE }

public class DrillingGame : Minigame
{
    //temp - jacques test
    [SerializeField] private DrillGameMap map;
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
    [SerializeField] private float joystickArrowFadeSpeed = 2f;
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Image joystickArrow;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image brokenDrillToast;
    [SerializeField] private UnityEngine.UI.Image brokenPipeToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;
    [SerializeField] private bool AutoWin;
    [SerializeField] private Animator animator;
    [SerializeField] public int[] columns;
    [SerializeField] public int[] rows;
    [SerializeField] private TextAsset[] easyLevels;
    [SerializeField] private TextAsset[] mediumLevels;
    [SerializeField] private TextAsset[] hardLevels;

    private Rigidbody2D myBody;
    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    private DrillingGameState state;
    private ToastType toastType;
    private Vector2 initDrillPos;
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int tileTweenId;
    private int curveId = 0;
    //bools
    private bool slidingLeft, makeDrill, imagesActivated, joystickShaken = false;
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
    private List<GameObject> bottomRow = new List<GameObject>();

    public ToastType ToastType { get { return toastType; } set { toastType = value; } }
    public DrillingGameState State { get { return state; } }
    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public DrillGameMap Map { get { return map; } }
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
    public bool JoystickJustMoved { get; private set; }

    void Awake()
    {
        CurrentInput = DrillingDirection.NONE;
        PrevInput = DrillingDirection.NONE;

        joystickArrow.color = new Color(1, 1, 1, 0);
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
        SucceededDrill = true;
        levelsCounter = 0;
//        GameManager.Instance.Joystick.JoystickPanel.transform.SetParent(mainPanel.transform, true);
//        GameManager.Instance.Joystick.JoystickPanel.GetComponent<UnityEngine.UI.Image>().rectTransform.anchoredPosition = new Vector2(500, -100);
//        GameManager.Instance.Joystick.JoystickPanel.transform.localScale = new Vector3(1, 1, 1);
    }

    public override void Update()
    {
        base.Update();
        updateInput();

        if (IsRunning && Timeleft <= 0.5f)
        {
            SucceededDrill = false;
            toastType = global::ToastType.NONE;
            state = DrillingGameState.STARTSTOPTOAST;
        }
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && state != DrillingGameState.INACTIVE)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
        }

        //joystick arrow
        if (joystickArrow.color.a > 0)
        {
            joystickArrow.color = new Color(1, 1, 1, joystickArrow.color.a - Time.deltaTime * joystickArrowFadeSpeed);
            joystickArrow.rectTransform.localPosition = GameManager.Instance.DrillingGame.Drill.rectTransform.localPosition;
        }
    }


    void FixedUpdate()
    {
        if (drill)
            updateState();

        if (Time.time - drillStuckChecked > drillStuckCooldown)
        {
            checkDrillerStuck();
            drillStuckChecked = Time.time;
        }
        updateProgressBars();
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        imagesActivated = true;
        drill.transform.SetAsLastSibling();
        if (animator) animator.SetBool("isSlidingLeft", false);
//        GameManager.Instance.Joystick.InnerPad.GetComponent<UnityEngine.UI.Image>().rectTransform.anchoredPosition = new Vector2(0, 0);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        stuckTimer = stuckTime;
        myBody.inertia = 0;
    }

    public void PointJoystickArrow(DrillingDirection direction)
    {
        joystickArrow.color = new Color(1, 1, 1, 1);

        float rotation = 0;

        switch (direction)
        {
            case DrillingDirection.UP:
                rotation = 0;
                break;
            case DrillingDirection.DOWN:
                rotation = 180;
                break;
            case DrillingDirection.LEFT:
                rotation = 90;
                break;
            case DrillingDirection.RIGHT:
                rotation = 270;
                break;
        }

        //        joystickArrow.rectTransform.localPosition = offset;
        joystickArrow.transform.localEulerAngles = new Vector3(0, 0, rotation);
        joystickArrow.transform.SetAsLastSibling();
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
                //generateLevel(tiles);  // pre-designed levels, loading from csv
                map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(easyLevels[levelsCounter]));
            }
            else if (levelsCounter >=3 && levelsCounter < 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(mediumLevels[(SucceededDrill == true) ? levelsCounter - 3 : Random.Range(3, 6)]);
                //generateLevel(tiles);  // pre-designed levels, loading from csv
            }
            else if (levelsCounter >= 6)
            {
                int[] tiles = GameManager.Instance.LoadDrillingPuzzle(hardLevels[(SucceededDrill == true) ? levelsCounter - 6 : Random.Range(6, 9)]);
                //generateLevel(tiles); // pre-designed levels, loading from csv
            }
            if (SucceededDrill) levelsCounter++;
            panelSlidingTimer = panelSlidingTime;
//            GameManager.Instance.Joystick.StartPosition = GameManager.Instance.Joystick.transform.position;
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
            activateToast(toastType);
            if (toastTimer > 0)
                LeanTween.move(steamImage.gameObject.GetComponent<RectTransform>(), new Vector3(0, 50, 0), toastMessageTime).setEase(LeanTweenType.easeOutQuad);
            else deactivateToast(true);
        }
        else
        {
            activateToast(toastType);
            if (toastTimer < 0.0f) deactivateToast(false);
        }
    }

    private void deactivateToast(bool gameSucceeded)
    {
        toastTimer = toastMessageTime;
        if (gameSucceeded)
        {
            endOkToast.gameObject.SetActive(false);
            LeanTween.move(steamImage.gameObject.GetComponent<RectTransform>(), new Vector3(0, -475, 0), toastMessageTime).setEase(LeanTweenType.easeOutQuad);
        }
        else
        {
            switch(toastType)
            {
                case global::ToastType.BROKEN_DRILL:
                    brokenDrillToast.gameObject.SetActive(false);
                    break;
                case global::ToastType.BROKEN_PIPE:
                    brokenPipeToast.gameObject.SetActive(false);
                    break;
                case global::ToastType.EXPLODED_BOMB:
                    //todo
                    break;
            }
            drillLife.fillAmount = 0f;
        }
        state = DrillingGameState.INACTIVE;

        End(gameSucceeded);
    }

    private void updateInput()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (GameManager.Instance.Joystick.JoystickInput.x >= 0.707f) // sin 45 deg
            {
                //right
                if (CurrentInput != DrillingDirection.RIGHT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.LEFT && CurrentInput != DrillingDirection.RIGHT)
                {
                    CurrentInput = DrillingDirection.RIGHT;
                    JoystickJustMoved = true;
                }
            }
            else if (GameManager.Instance.Joystick.JoystickInput.x <= -0.707) // sin 45 deg
            {
                //left
                if (CurrentInput != DrillingDirection.LEFT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.RIGHT && CurrentInput != DrillingDirection.LEFT)
                {
                    CurrentInput = DrillingDirection.LEFT;
                    JoystickJustMoved = true;
                }
            }
            else if (GameManager.Instance.Joystick.JoystickInput.y >= 0.707f) // cos 45 deg
            {
                //up
                if (CurrentInput != DrillingDirection.UP) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.DOWN && CurrentInput != DrillingDirection.UP)
                {
                    CurrentInput = DrillingDirection.UP;
                    JoystickJustMoved = true;
                }
            }
            else if (GameManager.Instance.Joystick.JoystickInput.y <= -0.707f) // cos 45 deg
            {
                //down
                if (CurrentInput != DrillingDirection.DOWN) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.UP && CurrentInput != DrillingDirection.DOWN)
                {
                    CurrentInput = DrillingDirection.DOWN;
                    JoystickJustMoved = true;
                }
            }
        }
        else if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (GameManager.Instance.Joystick.JoystickInput.y < -0.707f)
            {
                MakeDrill(true);
                Animator.SetBool("shouldJump", true);
                CurrentInput = DrillingDirection.DOWN;
                
            }
        }
    }

    private void handleDrillingState()
    {
        if (!ReachedBottom(636, drill)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void drillDown()
    {
        switch(PrevInput)
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
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 2;
                        targetColumn++;
                        animator.SetBool("isDrillingRight", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 2;
                    animator.SetBool("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
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
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 4;
                        targetColumn--;
                        animator.SetBool("isDrillingLeft", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 4;
                    animator.SetBool("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y <= rows[targetRow + 1])
                {
                    if(JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
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
        switch (PrevInput)
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
                        map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
                        curveId = 5;
                        targetRow++;
                        animator.SetBool("isDrillingDown", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != rows[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 5;
                    animator.SetBool("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
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
                        map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
                        curveId = 2;
                        targetRow--;
                        animator.SetBool("isDrillingUp", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 2;
                    animator.SetBool("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1])
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
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
        switch (PrevInput)
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
                        map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
                        curveId = 3;
                        map.FlashCoords = new Vector2((DrillGameMap.TILE_WIDTH * targetColumn) + DrillGameMap.TILE_WIDTH,
                            (DrillGameMap.TILE_HEIGHT * targetRow) + DrillGameMap.TILE_HEIGHT);
                        map.TriggerFlash = true;
                        targetRow++;
                        animator.SetBool("isDrillingDown", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != rows[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 3;
                    animator.SetBool("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
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
                        map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
                        curveId = 4;
                        targetRow--;
                        animator.SetBool("isDrillingUp", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.y != columns[targetRow])
                        drill.rectTransform.anchoredPosition = new Vector2(drill.rectTransform.anchoredPosition.x, rows[targetRow]);
                    curveId = 4;
                    animator.SetBool("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn < columns.Length - 1 && drill.rectTransform.anchoredPosition.x >= columns[targetColumn + 1])
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
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
        switch (PrevInput)
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
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 5;
                        targetColumn++;
                        animator.SetBool("isDrillingRight", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 5;
                    animator.SetBool("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
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
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 3;
                        targetColumn--;
                        animator.SetBool("isDrillingLeft", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (drill.rectTransform.anchoredPosition.x != columns[targetColumn])
                        drill.rectTransform.anchoredPosition = new Vector2(columns[targetColumn], drill.rectTransform.anchoredPosition.y);
                    curveId = 3;
                    animator.SetBool("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow > 0 && drill.rectTransform.anchoredPosition.y >= rows[targetRow - 1])
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow, 1, mapPanel);
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
        switch (CurrentInput)
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
        toastType = global::ToastType.SUCCESS;
        state = DrillingGameState.STARTSTOPTOAST;
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
            toastType = global::ToastType.BROKEN_DRILL;
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
            if (brokenDrillToast) brokenDrillToast.gameObject.SetActive(false);
            if (endOkToast) endOkToast.gameObject.SetActive(false);
            imagesActivated = false;
        }
    }

    private void resetGameGuts()
    {
        makeDrill = false;
        slidingLeft = false;
        joystickShaken = false;
        JoystickJustMoved = false;
        animator.SetBool("isSlidingLeft", false);
        animator.SetBool("isDrillingDown", false);
        animator.SetBool("isDrillingUp", false);
        animator.SetBool("isDrillingRight", false);
        animator.SetBool("isDrillingLeft", false);
        animator.SetBool("shouldJump", false);
        SucceededDrill = false;
        targetColumn = -1;
        targetRow = -1;
        drill.rectTransform.anchoredPosition = initDrillPos;
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        drill.color = new Color(1, 1, 1);
        drillLife.color = new Color(1, 1, 1);
        waterBar.fillAmount = 0f;
        drillLife.fillAmount = 1f;
        toastType = global::ToastType.NONE;

        map.Reset();
    }

    public void handleRockCollision(bool entered)
    {
        if (entered)
        {
            drill.color = new Color(1, 0, 0);
            drillLife.color = new Color(1, 0, 0);
        }
        else
        {
            drill.color = new Color(1, 1, 1);
            drillLife.color = new Color(1, 1, 1);
        }
        Bumped = entered;
    }

    public void handlePipeCollision()
    {
        SucceededDrill = false;
        state = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        toastType = global::ToastType.BROKEN_PIPE;
        drill.color = new Color(1, 0, 0);
        drillLife.color = new Color(1, 0, 0);
    }

    public void handleMineCollision()
    {
        SucceededDrill = false;
        state = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        toastType = global::ToastType.EXPLODED_BOMB;
        drill.color = new Color(1, 0, 0);
        drillLife.color = new Color(1, 0, 0);
    }

    public void handleMineAreaCollision()
    {
        SucceededDrill = false;
        state = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        toastType = global::ToastType.TRIGGERED_BOMB;
        drill.color = new Color(1, 0, 0);
        drillLife.color = new Color(1, 0, 0);
    }

    private void activateToast(ToastType type)
    {
        switch (type)
        {
            case global::ToastType.BROKEN_PIPE:
                brokenPipeToast.gameObject.SetActive(true);
                brokenPipeToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.BROKEN_DRILL:
                brokenDrillToast.gameObject.SetActive(true);
                brokenDrillToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.EXPLODED_BOMB:
                //todo
                break;
            case global::ToastType.TRIGGERED_BOMB:
                //todo
                break;
            case global::ToastType.SUCCESS:
                endOkToast.gameObject.SetActive(true);
                endOkToast.gameObject.transform.parent.SetAsLastSibling();
                break;
        }
    }
}
