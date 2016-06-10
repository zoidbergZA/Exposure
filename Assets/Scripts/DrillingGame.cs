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

    [SerializeField] private float toastMessageTime = 3.0f; //to be removed to mini-game hud
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;  //to be removed to mini-game hud
    [SerializeField] private float joystickArrowFadeSpeed = 2f;  //to be removed to mini-game hud
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image mainPanel;
    [SerializeField] private UnityEngine.UI.Image joystickArrow;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image endOkToast;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image brokenDrillToast;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image brokenPipeToast;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image waterBar;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image steamImage;  //to be removed to mini-game hud
    [SerializeField] private UnityEngine.UI.Image drillLife;  //to be removed to mini-game hud
    [SerializeField] private bool AutoWin;
    [SerializeField] private TextAsset[] levels;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
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
    private bool slidingLeft, makeDrill, imagesActivated, joystickShaken, reachedTile = false;
    //timers
    private float toastTimer;  //to be removed to mini-game hud
    private float jumpPhaseTimer;
    private float panelSlidingTimer;  //to be removed to mini-game hud

    public bool SucceededDrill { get; set; }
    public bool IsRestarting { get; set; }

    public ToastType ToastType { get { return toastType; } set { toastType = value; } }
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection CurrentInput { get; private set; }
    public DrillingDirection PrevInput { get; private set; }
    public DrillGameMap Map { get { return map; } }
    public Driller Driller { get; private set; }
    public DrillGameHud Hud { get; private set; }
    public UnityEngine.UI.Image WaterBar { get { return waterBar; } }
    public UnityEngine.UI.Image DrillLife { get { return drillLife; } }
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

        joystickArrow.color = new Color(1, 1, 1, 0);
    }

    void Start()
    {
        IsRestarting = false;
        activateImages(false);
        targetColumn = 0;
        targetRow = 0;
        toastTimer = toastMessageTime; //to be removed to mini-game hud
        jumpPhaseTimer = jumpPhaseTime;
        panelSlidingTimer = panelSlidingTime; //to be removed to mini-game hud
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height) - 700, 0);
        startDrillerPosition = Driller.Position;
        levelsCounter = 0;
    }

    public override void Update()
    {
        base.Update();
        updateInput();
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
//        Debug.Log("x: " + (int)Driller.Position.x + " | y: " + (int)Driller.Position.y + " | row: " + targetRow + " | col: " + targetColumn);
    }


    void FixedUpdate()
    {
        if (Driller.Drill)
            updateState();
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
        if(panelSlidingTimer <= 0 && !IsRestarting)
        {
            map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
            panelSlidingTimer = panelSlidingTime;
            
            //cheat flag to skip mini-game
            if (!AutoWin) state = DrillingGameState.SLIDING;
            else state = DrillingGameState.SUCCESS;
        }
        if(IsRestarting)
        {
            map.Initialize(mapPanel, GameManager.Instance.LoadDrillingPuzzle(levels[levelsCounter]));
            state = DrillingGameState.SLIDING;
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
            if (GameManager.Instance.Joystick.JoystickInput.x >= 0.707f && reachedTile) // sin 45 deg
            {
                //right
                if (CurrentInput != DrillingDirection.RIGHT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.LEFT && CurrentInput != DrillingDirection.RIGHT)
                {
                    CurrentInput = DrillingDirection.RIGHT;
                    JoystickJustMoved = true;
                }
            }
            if (GameManager.Instance.Joystick.JoystickInput.x <= -0.707 && reachedTile) // sin 45 deg
            {
                //left
                if (CurrentInput != DrillingDirection.LEFT) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.RIGHT && CurrentInput != DrillingDirection.LEFT)
                {
                    CurrentInput = DrillingDirection.LEFT;
                    JoystickJustMoved = true;
                }
            }
            if (GameManager.Instance.Joystick.JoystickInput.y >= 0.707f && reachedTile) // cos 45 deg
            {
                //up
                if (CurrentInput != DrillingDirection.UP) PrevInput = CurrentInput;
                if (PrevInput != DrillingDirection.DOWN && CurrentInput != DrillingDirection.UP)
                {
                    CurrentInput = DrillingDirection.UP;
                    JoystickJustMoved = true;
                }
            }
            if (GameManager.Instance.Joystick.JoystickInput.y <= -0.707f && reachedTile) // cos 45 deg
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
                if (Driller.Position.x < (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    Driller.Body.AddRelativeForce(new Vector2(1 * drillSpeed * Time.deltaTime, 0), ForceMode2D.Impulse); //drill right
                    Driller.Body.constraints = RigidbodyConstraints2D.FreezePositionY;
                }
                else
                {
                    map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = 2;
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow - 1, 0, mapPanel);
                    curveId = 4;
                    if (targetColumn > 0) targetColumn--;
                    reachedTile = true;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.y <= -(TILE_SIZE * targetRow) - TILE_SIZE)
                {
                    if(JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = 5;
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow - 1, 1, mapPanel);
                    curveId = 2;
                    if (targetRow > 0) targetRow--;
                    reachedTile = true;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.x <= (TILE_SIZE * targetColumn) - TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn > 0) targetColumn--;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    curveId = 3;
                    map.FlashCoords = new Vector2((TILE_SIZE * targetColumn) + TILE_SIZE, -(TILE_SIZE * targetRow) + TILE_SIZE);
                    map.TriggerFlash = true;
                    if (targetRow < MAP_HEIGHT - 1) targetRow++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    curveId = 4;
                    if (targetRow > 0) targetRow--;
                    reachedTile = true;
                    Driller.SwitchAnimation("isDrillingUp", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.x >= (TILE_SIZE * targetColumn) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    curveId = 5;
                    if (targetColumn < MAP_WIDTH - 1) targetColumn++;
                    reachedTile = true;
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
                    map.instantiatePipe(targetColumn, targetRow-1, 0, mapPanel);
                    curveId = 3;
                    if (targetColumn > 0) targetColumn--;
                    reachedTile = true;
                    Driller.SwitchAnimation("isDrillingLeft", false);
                    PrevInput = DrillingDirection.NONE;
                }
                break;
            case DrillingDirection.NONE:
                if (Driller.Position.y >= -(TILE_SIZE * targetRow) + TILE_SIZE)
                {
                    if (JoystickJustMoved)
                    {
                        map.instantiatePipe(targetColumn, targetRow-1, curveId, mapPanel);
                        JoystickJustMoved = false;
                    }
                    else map.instantiatePipe(targetColumn, targetRow-1, 1, mapPanel);
                    if (targetRow > 0) targetRow--;
                    reachedTile = true;
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
        //to do update drill life depending on amount of life
        drillLife.fillAmount = 1.00f;
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
        else GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        resetGameGuts();
    }

    private void RestartGame()
    {
        state = DrillingGameState.ACTIVATION;
        resetGameGuts();
    }

    //this function need refactoring, split to other functions or remove at all if unnecessary
    private void activateImages(bool activate)
    {
        if(activate) Driller.Drill.gameObject.SetActive(true);
        else
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
        Driller.Position = startDrillerPosition;
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height) - 700, 0), panelSlidingTime / 2);
        waterBar.fillAmount = 0f; //to be removed to mini-game-hud
        drillLife.fillAmount = 1f; //to be removed to mini-game-hud
        toastType = global::ToastType.NONE;

        map.Reset();
        Driller.Reset(IsRestarting);
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
