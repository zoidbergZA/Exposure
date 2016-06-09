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
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] public float stuckTime = 10.0f;
    [SerializeField] private float drillStuckCooldown = 2.0f;
    [SerializeField] private float joystickArrowFadeSpeed = 2f;
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private UnityEngine.UI.Image joystickArrow;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image brokenDrillToast;
    [SerializeField] private UnityEngine.UI.Image brokenPipeToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;
    [SerializeField] private bool AutoWin;
    [SerializeField] private TextAsset[] levels;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    public const int TILE_SIZE = 44, MAP_WIDTH = 19, MAP_HEIGHT = 14;
    private DrillingGameState state;
    private ToastType toastType;
    private Vector2 startDrillPosition;
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
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

    public ToastType ToastType { get { return toastType; } set { toastType = value; } }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public DrillGameMap Map { get { return map; } }
    public Driller Driller { get; private set; }
    public DrillGameHud Hud { get; private set; }
    public void MakeDrill(bool value) { makeDrill = value; }
    public float StuckTimer { get { return stuckTimer; } set { stuckTimer = value; } }
    public UnityEngine.UI.Image WaterBar { get { return waterBar; } }
    public UnityEngine.UI.Image DrillLife { get { return drillLife; } }
    public float DiamondValue { get { return diamondValue; } }
    public UnityEngine.UI.Image MainPanel { get { return mainPanel; } }
    public bool ReachedBottom(int bottom)
    {
        return Driller.Position.y <= startDrillPosition.y - bottom;
    }
    public bool JoystickJustMoved { get; private set; }

    void Awake()
    {
        CurrentInput = DrillingDirection.NONE;
        PrevInput = DrillingDirection.NONE;
        Driller = FindObjectOfType<Driller>();
        Hud = FindObjectOfType<DrillGameHud>();

        joystickArrow.color = new Color(1, 1, 1, 0);
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
        startDrillPosition = Driller.Position;
        SucceededDrill = true;
        levelsCounter = 0;
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
            joystickArrow.rectTransform.localPosition = GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.localPosition;
        }
        Debug.Log("x: " + (int)Driller.Position.x + " | y: " + (int)Driller.Position.y + " | row: " + targetRow + " | col: " + targetColumn);
    }


    void FixedUpdate()
    {
        if (Driller.Drill)
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
        Driller.Drill.transform.SetAsLastSibling();
        Driller.SwitchAnimation("isSlidingLeft", false);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        stuckTimer = stuckTime;
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

        //joystickArrow.rectTransform.position = Driller.Position;
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
            map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
            if (SucceededDrill) levelsCounter++;
            panelSlidingTimer = panelSlidingTime;
            
            //cheat flag to skip mini-game
            if (!AutoWin) state = DrillingGameState.SLIDING;
            else state = DrillingGameState.SUCCESS;
        }
        
    }

    private void handlePreDrillJump()
    {
        jumpPhaseTimer -= Time.deltaTime;
        if (jumpPhaseTimer <= 0)
        {
            Driller.SwitchAnimation("isDrillingDown", true);
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
                Driller.SwitchAnimation("shouldJump", true);
                CurrentInput = DrillingDirection.DOWN;
                
            }
        }
    }

    private void handleDrillingState()
    {
        if (!ReachedBottom((MAP_HEIGHT * TILE_SIZE) + TILE_SIZE)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void drillDown()
    {
        switch(PrevInput)
        {
            case DrillingDirection.RIGHT:
                if (targetColumn < MAP_WIDTH - 1 && !Bumped)
                {
                    if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 2;
                        targetColumn++;
                        Driller.SwitchAnimation("isDrillingRight", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.x != TILE_SIZE * targetColumn) Driller.Position = new Vector2(TILE_SIZE * targetColumn, Driller.Position.y);
                    curveId = 2;
                    Driller.SwitchAnimation("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.LEFT:
                if (targetColumn > 0 && !Bumped)
                {
                    if (Driller.Position.x > (TILE_SIZE * targetColumn) - TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow, 0, mapPanel);
                        curveId = 4;
                        targetColumn--;
                        Driller.SwitchAnimation("isDrillingLeft", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.x != TILE_SIZE * targetColumn) Driller.Position = new Vector2(TILE_SIZE * targetColumn, Driller.Position.y);
                    curveId = 4;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow < MAP_HEIGHT - 1 && Driller.Position.y <= -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    if(JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    targetRow++;
                }
                if (!Driller.Animator.GetBool("isDrillingDown")) Driller.SwitchAnimation("isDrillingDown", true);
                Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                break;
        }
    }

    private void drillLeft()
    {
        switch (PrevInput)
        {
            case DrillingDirection.DOWN:
                if (targetRow < MAP_HEIGHT - 1 && !Bumped)
                {
                    if (Driller.Position.y > -(TILE_SIZE * targetRow) - TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                        curveId = 5;
                        targetRow++;
                        Driller.SwitchAnimation("isDrillingDown", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.y != -(TILE_SIZE * targetRow)) Driller.Position = new Vector2(Driller.Position.x, -(TILE_SIZE * targetRow));
                    curveId = 5;
                    Driller.SwitchAnimation("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.UP:
                if (targetRow > 0 && !Bumped)
                {
                    if (Driller.Position.y < -(TILE_SIZE * targetRow) + TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                        curveId = 2;
                        targetRow--;
                        Driller.SwitchAnimation("isDrillingUp", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.y != -(TILE_SIZE * targetRow)) Driller.Position = new Vector2(Driller.Position.x, -(TILE_SIZE * targetRow));
                    curveId = 2;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn > 0 && Driller.Position.x <= (TILE_SIZE * targetColumn) - TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    targetColumn -= 1;
                }
                if (!Driller.Animator.GetBool("isDrillingLeft")) Driller.SwitchAnimation("isDrillingLeft", true);
                Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                break;
        }
    }

    private void drillRight()
    {
        switch (PrevInput)
        {
            case DrillingDirection.DOWN:
                if (targetRow < MAP_HEIGHT - 1 && !Bumped)
                {
                    if (Driller.Position.y > -(TILE_SIZE * targetRow) - TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                        curveId = 3;
                        map.FlashCoords = new Vector2((TILE_SIZE * targetColumn) + TILE_SIZE, (TILE_SIZE * targetRow) + TILE_SIZE);
                        map.TriggerFlash = true;
                        targetRow++;
                        Driller.SwitchAnimation("isDrillingDown", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.y != -(TILE_SIZE * targetRow)) Driller.Position = new Vector2(Driller.Position.x, -(TILE_SIZE * targetRow));
                    curveId = 3;
                    Driller.SwitchAnimation("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.UP:
                if (targetRow > 0 && !Bumped)
                {
                    if (Driller.Position.y < -(TILE_SIZE * targetRow) + TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                        curveId = 4;
                        targetRow--;
                        Driller.SwitchAnimation("isDrillingUp", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.y != -(TILE_SIZE * targetRow)) Driller.Position = new Vector2(Driller.Position.x, -(TILE_SIZE * targetRow));
                    curveId = 4;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetColumn < MAP_WIDTH - 1 && Driller.Position.x >= (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    targetColumn += 1;
                }
                if (!Driller.Animator.GetBool("isDrillingRight")) Driller.SwitchAnimation("isDrillingRight", true);
                Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                break;
        }
    }

    private void drillUp()
    {
        switch (PrevInput)
        {
            case DrillingDirection.RIGHT:
                if (targetColumn < MAP_WIDTH - 1 && !Bumped)
                {
                    if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                        curveId = 5;
                        targetColumn++;
                        Driller.SwitchAnimation("isDrillingRight", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.x != TILE_SIZE * targetColumn) Driller.Position = new Vector2(TILE_SIZE * targetColumn, Driller.Position.y);
                    curveId = 5;
                    Driller.SwitchAnimation("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.LEFT:
                if (targetColumn > 0 && !Bumped)
                {
                    if (Driller.Position.x > (TILE_SIZE * targetColumn) - TILE_SIZE)
                    {
                        Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                        Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                    }
                    else
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                        curveId = 3;
                        targetColumn--;
                        Driller.SwitchAnimation("isDrillingLeft", false);
                        PrevInput = DrillingDirection.NONE;
                    }
                }
                else
                {
                    if (Driller.Position.x != TILE_SIZE * targetColumn) Driller.Position = new Vector2(TILE_SIZE * targetColumn, Driller.Position.y);
                    curveId = 3;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (targetRow > 0 && Driller.Position.y >= -(TILE_SIZE * targetRow) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    targetRow--;
                }
                if (!Driller.Animator.GetBool("isDrillingUp")) Driller.SwitchAnimation("isDrillingUp", true);
                Driller.Body.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
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

    private void updateSlidingMovement()
    {
        if (!makeDrill)
        {
            if (slidingLeft == false)
            {
                if (targetColumn < MAP_WIDTH - 1)
                {
                    if (Driller.Position.x >= (TILE_SIZE * targetColumn) + TILE_SIZE) targetColumn += 1;
                }
                else
                {
                    slidingLeft = true;
                    Driller.SwitchAnimation("isSlidingLeft", true);
                }
                Driller.Drill.transform.Translate(new Vector3(1 * slideSpeed * Time.deltaTime, 0, 0));
            }
            else
            {
                if (targetColumn > 0)
                {
                    if (Driller.Position.x <= (TILE_SIZE * targetColumn) - TILE_SIZE) targetColumn -= 1;
                }
                else
                {
                    slidingLeft = false;
                    Driller.SwitchAnimation("isSlidingLeft", false);
                }
                Driller.Drill.transform.Translate(new Vector3(-1 * slideSpeed * Time.deltaTime, 0, 0));
            }
        }
        else
        {
            if (slidingLeft == false)
            {
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                    Driller.Position = new Vector2((TILE_SIZE * targetColumn) + TILE_SIZE, Driller.Position.y);
                else
                {
                    state = DrillingGameState.PREDRILLJUMP;
                    if (targetColumn < MAP_WIDTH) targetColumn += 1;
                }
            }
            else
            {
                if (Driller.Position.x > (TILE_SIZE * targetColumn) - TILE_SIZE)
                    Driller.Position = new Vector2((TILE_SIZE * targetColumn) - TILE_SIZE, Driller.Position.y);
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
        if (waterBar && map.GetWaterCount <= 3) waterBar.fillAmount = map.GetWaterCount * 33.33333334f / 100f;
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
        }
        else GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        resetGameGuts();
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            Driller.Drill.gameObject.SetActive(true);
        } else
        {
            Driller.Drill.gameObject.SetActive(false);
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
        SucceededDrill = false;
        targetColumn = 0;
        targetRow = 0;
        Driller.Position = startDrillPosition;
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        Driller.Drill.color = new Color(1, 1, 1);
        drillLife.color = new Color(1, 1, 1);
        waterBar.fillAmount = 0f;
        drillLife.fillAmount = 1f;
        toastType = global::ToastType.NONE;

        map.Reset();
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
