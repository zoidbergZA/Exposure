using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public enum DrillingDirection { UP, DOWN, LEFT, RIGHT, NONE }
public enum ToastType { SUCCESS, BROKEN_DRILL, BROKEN_PIPE, EXPLODED_BOMB, TRIGGERED_BOMB, NONE }

public class DrillingGame : Minigame
{
    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private RectTransform MainPanel;
    [SerializeField] private float drillSpeed = 90.0f;
    [SerializeField] private float slideSpeed = 80.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float jumpPhaseTime = 0.85f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] private MovementType movementType;
    [SerializeField] private TextAsset[] levels;
    [SerializeField] private TextAsset[] JsonLevels;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, ACTIVATION, SLIDING, PREDRILLJUMP, DRILLING, SUCCESS, FAIL, RESTART }
    public enum MovementType { CONSTANT, TILE_BASED }
    public const int TILE_SIZE = 71, MAP_WIDTH = 13, MAP_HEIGHT = 8;
    private DrillingGameState state;
    private Vector2 startDrillerPosition;
    private Vector2 mainPanelActivePosition = new Vector2(0, 100);
    private Vector2 mainPanelInactivePosition = new Vector2(0, -(Screen.height) - 700);
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int curveId = 0;
    private bool slidingLeft, makeDrill = false;
    private float jumpPhaseTimer;

    public bool IsRestarting { get; set; }
    public ToastType ToastType { get; set; }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection DrillDirection { get; private set; }
    public DrillingDirection PrevDrillDirection { get; private set; }
    public DrillGameMap Map { get; private set; }
    public Driller Driller { get; private set; }
    public DrillGameHud Hud { get; private set; }
    public MobileJoystick Joystick { get; private set; }
    public float DiamondValue { get { return diamondValue; } }
    public bool ReachedBottom(int bottom) { return Driller.Position.y <= startDrillerPosition.y - bottom; }
    public bool JoystickJustMoved { get; private set; }

    void Awake()
    {
        DrillDirection = DrillingDirection.NONE;
        PrevDrillDirection = DrillingDirection.NONE;
        Driller = FindObjectOfType<Driller>();
        Hud = FindObjectOfType<DrillGameHud>();
        Map = FindObjectOfType<DrillGameMap>();
        Joystick = FindObjectOfType<MobileJoystick>();
    }

    void Start()
    {
        state = DrillingGameState.INACTIVE;
        ToastType = global::ToastType.NONE;
        targetColumn = 0;
        targetRow = 0;
        jumpPhaseTimer = jumpPhaseTime;
        MainPanel.anchoredPosition = mainPanelInactivePosition;
        startDrillerPosition = Driller.Position;
        levelsCounter = 0;
        Hud.JoystickArrow.color = new Color(1, 1, 1, 0);
    }

    public override void Update()
    {
        base.Update();
        processJoystickInput();
        if (Driller.Drill) updateState();

        //cheat buttons
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (levelsCounter < levels.Length-1) levelsCounter++;
            else levelsCounter = 0;
        }
        if (Input.GetKeyDown(KeyCode.N)) Driller.Body.mass += 0.01f;
        if (Input.GetKeyDown(KeyCode.M)) Driller.Body.mass -= 0.01f;
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        LeanTween.move(MainPanel, mainPanelActivePosition, panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
        Joystick.Reset();
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
                //todo if necessary
                break;
            case DrillingGameState.SUCCESS:
                handleSuccessState();
                break;
            case DrillingGameState.PREDRILLJUMP:
                handlePreDrillJump();
                break;
            case DrillingGameState.ACTIVATION:
                handleActivation();
                break;
            case DrillingGameState.RESTART:
                handleRestart();
                break;
            case DrillingGameState.FAIL:
                handleFail();
                break;
        }
    }

    //SECTION WITH STATES HANDLING FUNCTIONS
    #region
    private void handleActivation()
    {
        Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]), JsonLevels[levelsCounter]);
        Driller.Drill.gameObject.SetActive(true);
        Driller.Drill.transform.SetAsLastSibling();

        //cheat flag to skip mini-game
        if (!GameManager.Instance.MiniGameAutoWin) state = DrillingGameState.SLIDING;
        else state = DrillingGameState.SUCCESS;
    }

    private void handleSlidingState()
    {
        updateSlidingMovement();
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

    private void handleDrillingState()
    {
        if (!ReachedBottom((MAP_HEIGHT * TILE_SIZE) + TILE_SIZE / 2))
        {
            switch(movementType)
            {
                case MovementType.CONSTANT:
                    updateDrilling();
                    break;
                case MovementType.TILE_BASED:
                    updateTileBasedDrilling();
                    break;
                default:
                    updateDrilling();
                    break;
            }
        }
        else
        {
            Hud.ActivateToast(ToastType.SUCCESS);
            state = DrillingGameState.SUCCESS;
        }
        if(Driller.Collided)
        {
            Hud.ActivateToast(ToastType);
            if (Driller.Lives <= 0) state = DrillingGameState.FAIL;
            else state = DrillingGameState.RESTART;
        }
    }

    private void handleSuccessState()
    {
        Hud.ToastTimer -= Time.deltaTime;

        if (Hud.ToastTimer <= 0)
        {
            Hud.DeactivateToast(ToastType.SUCCESS);
            IsRestarting = false;
            End(true);
            state = DrillingGameState.INACTIVE;
        }
    }

    private void handleRestart()
    {
        Hud.ToastTimer -= Time.deltaTime;

        if (Hud.ToastTimer <= 0)
        {
            IsRestarting = true;
            Hud.DeactivateToast(ToastType);
            resetGame();
            Joystick.Reset();
            Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]), JsonLevels[levelsCounter]);
            Driller.Drill.gameObject.SetActive(true);
            Driller.Drill.transform.SetAsLastSibling();
            state = DrillingGameState.SLIDING;
        }
    }

    private void handleFail()
    {
        Hud.ToastTimer -= Time.deltaTime;
        if (Hud.ToastTimer <= 0)
        {
            Hud.DeactivateToast(ToastType);
            IsRestarting = false;
            End(false);
            state = DrillingGameState.INACTIVE;
        }
    }
    #endregion

    private void processJoystickInput()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            switch(GameManager.Instance.Joystick.CurrentInput)
            {
                case DrillingDirection.RIGHT:
                    if (PrevDrillDirection != DrillingDirection.LEFT && PrevDrillDirection != DrillingDirection.RIGHT
                        && DrillDirection != DrillingDirection.LEFT && DrillDirection != DrillingDirection.RIGHT)
                    {
                        PrevDrillDirection = DrillDirection;
                        DrillDirection = DrillingDirection.RIGHT;
                        JoystickJustMoved = true;
                    }
                    break;
                case DrillingDirection.LEFT:
                    if (PrevDrillDirection != DrillingDirection.RIGHT && PrevDrillDirection != DrillingDirection.LEFT
                        && DrillDirection != DrillingDirection.RIGHT && DrillDirection != DrillingDirection.LEFT)
                    {
                        PrevDrillDirection = DrillDirection;
                        DrillDirection = DrillingDirection.LEFT;
                        JoystickJustMoved = true;
                    }
                    break;
                case DrillingDirection.UP:
                    if (PrevDrillDirection != DrillingDirection.DOWN && PrevDrillDirection != DrillingDirection.UP
                        && DrillDirection != DrillingDirection.DOWN && DrillDirection != DrillingDirection.UP)
                    {
                        PrevDrillDirection = DrillDirection;
                        DrillDirection = DrillingDirection.UP;
                        JoystickJustMoved = true;
                    }
                    break;
                case DrillingDirection.DOWN:
                    if (PrevDrillDirection != DrillingDirection.UP && PrevDrillDirection != DrillingDirection.DOWN
                        && DrillDirection != DrillingDirection.UP && DrillDirection != DrillingDirection.DOWN)
                    {
                        PrevDrillDirection = DrillDirection;
                        DrillDirection = DrillingDirection.DOWN;
                        JoystickJustMoved = true;
                    }
                    break;
                default:
                    Debug.Log("Wrong has been passed from joystick!");
                    break;
            }
        }
        else if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (GameManager.Instance.Joystick.CurrentInput == DrillingDirection.DOWN)
            {
                makeDrill = true;
                Driller.SwitchAnimation("shouldJump", true);
                DrillDirection = DrillingDirection.DOWN;
            }
        }
    }

    private void drillDown()
    {
        switch(PrevDrillDirection)
        {
            case DrillingDirection.RIGHT:
                processPreviousDirection(DrillingDirection.RIGHT, 2, new Vector2(0, -TILE_SIZE));
                break;
            case DrillingDirection.LEFT:
                processPreviousDirection(DrillingDirection.LEFT, 4, new Vector2(0, -TILE_SIZE));
                break;
            case DrillingDirection.NONE:
                if (!Driller.Animator.GetBool("isDrillingDown")) Driller.SwitchAnimation("isDrillingDown", true);
                if (Driller.Position.y <= -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    //if(JoystickJustMoved)
                    //{
                    //    Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                    //    JoystickJustMoved = false;
                    //}
                    //else Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                }
                Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                break;
        }
    }

    private void drillLeft()
    {
        switch (PrevDrillDirection)
        {
            case DrillingDirection.DOWN:
                processPreviousDirection(DrillingDirection.DOWN, 5, new Vector2(-TILE_SIZE, 0));
                break;
            case DrillingDirection.UP:
                processPreviousDirection(DrillingDirection.UP, 2, new Vector2(-TILE_SIZE, 0));
                break;
            case DrillingDirection.NONE:
                if (!Driller.Animator.GetBool("isDrillingLeft")) Driller.SwitchAnimation("isDrillingLeft", true);
                if (Driller.Position.x <= (TILE_SIZE * targetColumn) - TILE_SIZE)
                {
                    //if (JoystickJustMoved)
                    //{
                    //    Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                    //    JoystickJustMoved = false;
                    //}
                    //else Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn > 0) targetColumn--;
                }
                Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                break;
        }
    }

    private void drillRight()
    {
        switch (PrevDrillDirection)
        {
            case DrillingDirection.DOWN:
                processPreviousDirection(DrillingDirection.DOWN, 3, new Vector2(TILE_SIZE, 0));
                break;
            case DrillingDirection.UP:
                processPreviousDirection(DrillingDirection.UP, 4, new Vector2(TILE_SIZE, 0));
                break;
            case DrillingDirection.NONE:
                if (!Driller.Animator.GetBool("isDrillingRight")) Driller.SwitchAnimation("isDrillingRight", true);
                if (Driller.Position.x >= (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    //if (JoystickJustMoved)
                    //{
                    //    Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                    //    JoystickJustMoved = false;
                    //}
                    //else Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                }
                Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                break;
        }
    }

    private void drillUp()
    {
        switch (PrevDrillDirection)
        {
            case DrillingDirection.RIGHT:
                processPreviousDirection(DrillingDirection.RIGHT, 5, new Vector2(0, TILE_SIZE));
                break;
            case DrillingDirection.LEFT:
                processPreviousDirection(DrillingDirection.LEFT, 3, new Vector2(0, TILE_SIZE));
                break;
            case DrillingDirection.NONE:
                if (!Driller.Animator.GetBool("isDrillingUp")) Driller.SwitchAnimation("isDrillingUp", true);
                if (Driller.Position.y >= -(TILE_SIZE * targetRow) + TILE_SIZE)
                {
                    //if (JoystickJustMoved)
                    //{
                    //    Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                    //    JoystickJustMoved = false;
                    //}
                    //else Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow > 0) targetRow--;
                }
                Driller.Body.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                break;
        }
    }
    
    private void updateDrilling()
    {
        switch (DrillDirection)
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

    private void processPreviousDirection(DrillingDirection prevDir, int pCurveId, Vector2 flashTileOffset)
    {
        switch(prevDir)
        {
            case DrillingDirection.RIGHT:
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    Driller.SwitchAnimation("isDrillingRight", false);
                    //Map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = pCurveId;
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    Map.DoFlashTile(new Vector2(Driller.Position.x + flashTileOffset.x, Driller.Position.y + flashTileOffset.y));
                    PrevDrillDirection = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.LEFT:
                if (Driller.Position.x > (TILE_SIZE * targetColumn) - TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    //Map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = pCurveId;
                    if (targetColumn > 0) targetColumn--;
                    Map.DoFlashTile(new Vector2(Driller.Position.x + flashTileOffset.x, Driller.Position.y + flashTileOffset.y));
                    PrevDrillDirection = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.DOWN:
                if (Driller.Position.y > -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                }
                else
                {
                    Driller.SwitchAnimation("isDrillingDown", false);
                    //Map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = pCurveId;
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    Map.DoFlashTile(new Vector2(Driller.Position.x + flashTileOffset.x, Driller.Position.y + flashTileOffset.y));
                    PrevDrillDirection = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.UP:
                if (Driller.Position.y < -(TILE_SIZE * targetRow) + TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(0, 1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill up
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                }
                else
                {
                    Driller.SwitchAnimation("isDrillingUp", false);
                    //Map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = pCurveId;
                    if (targetRow > 0) targetRow--;
                    Map.DoFlashTile(new Vector2(Driller.Position.x + flashTileOffset.x, Driller.Position.y + flashTileOffset.y));
                    PrevDrillDirection = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                break;
            default:
                break;
        }
    }

    private void updateTileBasedDrilling()
    {
        switch (DrillDirection)
        {
            case DrillingDirection.DOWN:
                break;
            case DrillingDirection.LEFT:
                break;
            case DrillingDirection.RIGHT:
                break;
            case DrillingDirection.UP:
                break;
            case DrillingDirection.NONE:
                break;
            default:
                break;
        }
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
                Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
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
                Driller.Body.AddRelativeForce(new Vector2(-1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill left
                Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
            }
        }
        else
        {
            if (slidingLeft == false)
            {
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE) Driller.Position = new Vector2((TILE_SIZE * targetColumn) + TILE_SIZE, Driller.Position.y);
                else
                {
                    if (targetColumn < MAP_WIDTH) targetColumn += 1;
                    state = DrillingGameState.PREDRILLJUMP;
                }
            }
            else
            {
                if (Driller.Position.x > (TILE_SIZE * targetColumn) - TILE_SIZE) Driller.Position = new Vector2((TILE_SIZE * targetColumn) - TILE_SIZE, Driller.Position.y);
                else
                {
                    if (targetColumn > 0) targetColumn -= 1;
                    state = DrillingGameState.PREDRILLJUMP;
                }
            }
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GameManager.Instance.GridBuilder.Begin(1f);
            if (levelsCounter < levels.Length-1) levelsCounter++;
            else levelsCounter = 0;
        }
        else
        {
            GameManager.Instance.GridBuilder.PuzzlePath.Reset();
            
            //set scanner position to geoPlant
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.GridBuilder.PuzzlePath.ConnectablePath[0].transform.position);
        }
        resetGame();
    }

    private void resetGame()
    {
        makeDrill = false;
        slidingLeft = false;
        JoystickJustMoved = false;
        targetColumn = 0;
        targetRow = 0;
        if(!IsRestarting) LeanTween.move(MainPanel, mainPanelInactivePosition, panelSlidingTime);
        ToastType = global::ToastType.NONE;
        DrillDirection = DrillingDirection.NONE;
        PrevDrillDirection = DrillingDirection.NONE;

        Map.Reset();
        Driller.Reset(startDrillerPosition);
        Hud.Reset();
        Joystick.Reset();
    }
}
