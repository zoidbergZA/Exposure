using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DrillingDirection { UP, DOWN, LEFT, RIGHT, NONE }
public enum ToastType { SUCCESS, BROKEN_DRILL, BROKEN_PIPE, EXPLODED_BOMB, TRIGGERED_BOMB, NONE }

public class DrillingGame : Minigame
{
    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private bool AutoWin;
    [SerializeField] private TextAsset[] levels;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, ACTIVATION, SLIDING, PREDRILLJUMP, DRILLING, SUCCESS, FAIL, STARTSTOPTOAST, RESTART }
    public const int TILE_SIZE = 70, MAP_WIDTH = 12, MAP_HEIGHT = 9;
    private DrillingGameState state;
    private ToastType toastType;
    private Vector2 startDrillerPosition;
    private int targetColumn;
    private int targetRow;
    private int levelsCounter = 0;
    private int curveId = 0;
    private int lives = 3;
    //bools
    private bool slidingLeft, makeDrill, joystickShaken = false;
    //timers
    private float jumpPhaseTimer;

    public bool SucceededDrill { get; set; }
    public bool IsRestarting { get; set; }

    public ToastType ToastType { get { return toastType; } set { toastType = value; } }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public DrillGameMap Map { get; private set; }
    public Driller Driller { get; private set; }
    public DrillGameHud Hud { get; private set; }
    public float DiamondValue { get { return diamondValue; } }
    public UnityEngine.UI.Image MainPanel { get { return mainPanel; } }
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
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height) - 700, 0);
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
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
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

    private void handleInactiveState()
    {

    }

    private void handleActivation()
    {
        Map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
        Driller.Drill.gameObject.SetActive(true);
        Driller.Drill.transform.SetAsLastSibling();
        Driller.SwitchAnimation("isSlidingLeft", false);

        //cheat flag to skip mini-game
        if (!AutoWin) state = DrillingGameState.SLIDING;
        else state = DrillingGameState.SUCCESS;
    }

    private void handleSlidingState()
    {
        if (!joystickShaken)
        {
            LeanTween.scale(GameManager.Instance.Joystick.JoystickPanel.GetComponent<RectTransform>(),
                GameManager.Instance.Joystick.JoystickPanel.GetComponent<RectTransform>().localScale * 1.2f, 3)
                .setEase(LeanTweenType.punch);
            joystickShaken = true;
        }
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
        if (!ReachedBottom((MAP_HEIGHT * TILE_SIZE) + TILE_SIZE)) updateDrilling();
        else state = DrillingGameState.SUCCESS;
    }

    private void handleStartStopState()
    {
        Hud.ToastTimer -= Time.deltaTime;
        if (SucceededDrill)
        {
            Hud.ActivateToast(toastType);
            if (Hud.ToastTimer > 0)
                LeanTween.move(Hud.SteamImage.gameObject.GetComponent<RectTransform>(),
                    new Vector3(0, 50, 0), Hud.ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            else Hud.DeactivateToast(global::ToastType.SUCCESS);
        }
        else
        {
            Hud.ActivateToast(toastType);
            if (Hud.ToastTimer < 0.0f) Hud.DeactivateToast(global::ToastType.BROKEN_DRILL);
        }
    }

    private void handleSuccessState()
    {
        SucceededDrill = true;
        toastType = global::ToastType.SUCCESS;
        state = DrillingGameState.STARTSTOPTOAST;
    }

    private void handleRestart()
    {

    }

    private void handleFail()
    {
        End(false);
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
        joystickShaken = false;
        JoystickJustMoved = false;
        SucceededDrill = false;
        targetColumn = 0;
        targetRow = 0;
        Driller.Position = startDrillerPosition;
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        Hud.WaterBar.fillAmount = 0f; //to be removed to mini-game-hud
        Hud.DrillLife.fillAmount = 1f; //to be removed to mini-game-hud
        toastType = global::ToastType.NONE;

        Map.Reset();
        Driller.Reset(IsRestarting);
    }
}
