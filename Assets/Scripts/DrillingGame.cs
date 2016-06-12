using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private float panelSlidingTime = 1.0f;
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private bool AutoWin;
    [SerializeField] private TextAsset[] levels;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, ACTIVATION, SLIDING, PREDRILLJUMP, DRILLING, SUCCESS, FAIL, RESTART }
    public const int TILE_SIZE = 70, MAP_WIDTH = 12, MAP_HEIGHT = 9;
    private DrillingGameState state;
    private ToastType toastType;
    private Vector2 startDrillerPosition;
    private Vector2 mainPanelActivePosition = new Vector2(0, 100);
    private Vector2 mainPanelInactivePosition = new Vector2(0, -(Screen.height) - 700);
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int curveId = 0;
    private int lives = 3;
    //bools
    private bool slidingLeft, makeDrill = false;
    //timers
    private float jumpPhaseTimer;

    public bool IsRestarting { get; set; }

    public ToastType ToastType { get { return toastType; } set { toastType = value; } }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public DrillGameMap Map { get; private set; }
    public Driller Driller { get; private set; }
    public DrillGameHud Hud { get; private set; }
    public float DiamondValue { get { return diamondValue; } }
    public bool ReachedBottom(int bottom)
    {
        return Driller.Position.y <= startDrillerPosition.y - bottom;
    }
    public bool JoystickJustMoved { get; private set; }

    void Awake()
    {
        CurrentInput = DrillingDirection.NONE;
        PrevInput = DrillingDirection.NONE;
        Driller = FindObjectOfType<Driller>();
        Hud = FindObjectOfType<DrillGameHud>();
        Map = FindObjectOfType<DrillGameMap>();
    }

    void Start()
    {
        state = DrillingGameState.INACTIVE;
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
        updateInput();
        if (Driller.Drill) updateState();
        //Debug.Log("x: " + (int)Driller.Position.x + " | y: " + (int)Driller.Position.y + " | row: " + targetRow + " | col: " + targetColumn);
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        LeanTween.move(MainPanel, mainPanelActivePosition, panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
        state = DrillingGameState.ACTIVATION;
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

    //tested and finalized
    private void handleActivation()
    {
        if (levelsCounter < levels.Length) Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
        else Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[Random.Range(0, levels.Length)]));
        Driller.Drill.gameObject.SetActive(true);
        Driller.Drill.transform.SetAsLastSibling();

        //cheat flag to skip mini-game
        if (!AutoWin) state = DrillingGameState.SLIDING;
        else state = DrillingGameState.SUCCESS;
    }

    //tested and finalized
    private void handleSlidingState()
    {
        updateSlidingMovement();
    }

    //tested and finalized
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
        if (!ReachedBottom(MAP_HEIGHT * TILE_SIZE)) updateDrilling();
        else
        {
            Hud.ActivateToast(ToastType.SUCCESS);
            state = DrillingGameState.SUCCESS;
        }
        if(Driller.Collided)
        {
            Hud.ActivateToast(toastType);
            if (Driller.Lives <= 0) state = DrillingGameState.FAIL;
            else state = DrillingGameState.RESTART;
        }
    }

    //tested and finalized
    private void handleSuccessState()
    {
        Hud.ToastTimer -= Time.deltaTime;

        if (Hud.ToastTimer <= 0)
        {
            Hud.DeactivateToast(ToastType.SUCCESS);
            End(true);
            state = DrillingGameState.INACTIVE;
        }
    }

    private void handleRestart()
    {
        Hud.ToastTimer -= Time.deltaTime;

        if (Hud.ToastTimer <= 0)
        {
            Hud.DeactivateToast(toastType);
            IsRestarting = true;
            resetGame();
            Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
            Driller.Drill.gameObject.SetActive(true);
            Driller.Drill.transform.SetAsLastSibling();
            state = DrillingGameState.SLIDING;
        }
    }

    //tested and finalized
    private void handleFail()
    {
        Hud.ToastTimer -= Time.deltaTime;

        if (Hud.ToastTimer <= 0)
        {
            Hud.DeactivateToast(toastType);
            IsRestarting = false;
            End(false);
            state = DrillingGameState.INACTIVE;
        }
    }

    #endregion

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
            if (GameManager.Instance.Joystick.JoystickInput.x <= -0.707) // sin 45 deg
            {
                //left
                if (CurrentInput != DrillingDirection.LEFT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.RIGHT && CurrentInput != DrillingDirection.LEFT)
                {
                    CurrentInput = DrillingDirection.LEFT;
                    JoystickJustMoved = true;
                }
            }
            if (GameManager.Instance.Joystick.JoystickInput.y >= 0.707f) // cos 45 deg
            {
                //up
                if (CurrentInput != DrillingDirection.UP) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.DOWN && CurrentInput != DrillingDirection.UP)
                {
                    CurrentInput = DrillingDirection.UP;
                    JoystickJustMoved = true;
                }
            }
            if (GameManager.Instance.Joystick.JoystickInput.y <= -0.707f) // cos 45 deg
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
                makeDrill = true;
                Driller.SwitchAnimation("shouldJump", true);
                CurrentInput = DrillingDirection.DOWN;
            }
        }
    }

    private void drillDown()
    {
        switch(PrevInput)
        {
            case DrillingDirection.RIGHT:
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    Map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = 2;
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    Driller.SwitchAnimation("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
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
                    Map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = 4;
                    if (targetColumn > 0) targetColumn--;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.y <= -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    if(JoystickJustMoved)
                    {
                        Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
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
                if (Driller.Position.y > -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                }
                else
                {
                    Map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = 5;
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    Driller.SwitchAnimation("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
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
                    Map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = 2;
                    if (targetRow > 0) targetRow--;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.x <= (TILE_SIZE * targetColumn) - TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn > 0) targetColumn--;
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
                if (Driller.Position.y > -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(0, -1 * drillSpeed * Time.deltaTime), ForceMode2D.Impulse); //drill down
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionX;
                }
                else
                {
                    Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    curveId = 3;
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    Driller.SwitchAnimation("isDrillingDown", false);
                    PrevInput = DrillingDirection.NONE;
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
                    Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    curveId = 4;
                    if (targetRow > 0) targetRow--;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.x >= (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
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
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    curveId = 5;
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    Driller.SwitchAnimation("isDrillingRight", false);
                    PrevInput = DrillingDirection.NONE;
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
                    Map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    curveId = 3;
                    if (targetColumn > 0) targetColumn--;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.y >= -(TILE_SIZE * targetRow) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        Map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else Map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow > 0) targetRow--;
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
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            plant.transform.SetParent(GameManager.Instance.PlanetTransform);
            GameManager.Instance.Player.StartBuildMinigame(plant, 1f);
            levelsCounter++;
        }
        else
        {
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
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
        toastType = global::ToastType.NONE;

        Map.Reset();
        Driller.Reset(startDrillerPosition);
        Hud.Reset();
    }
}
