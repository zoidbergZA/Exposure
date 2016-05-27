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
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Image globeDrillGroundIcon;
    [SerializeField] private UnityEngine.UI.Image globeDrillPipeIcon;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image endFailToast;
    [SerializeField] private int[] columns;
    [SerializeField] private int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private bool AutoWin;
    [SerializeField] private float toastMessageTime = 3.0f;
    [SerializeField] private float drillSpeed = 3.0f;
    [SerializeField] private float slideSpeed = 1.0f;
    [SerializeField] private float diamondValue = 1.0f;
    [SerializeField] private float succeededDrillValue = 5.0f;
    [SerializeField] private float jumpPhaseTime = 0.25f;
    [SerializeField] private float panelSlidingTime = 1.5f;
    [SerializeField] private Animator animator;
    [SerializeField] private MobileJoystick joystick;

    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST, PREDRILLJUMP, ACTIVATION }
    public enum DrillingDirection {  UP, DOWN, LEFT, RIGHT }
    private DrillingGameState state;
    private DrillingDirection drillDir;
    private DrillingDirection prevDrillDir;
    private Vector3 initDrillPos;
    private int targetColumn;
    private int targetRow;
    //bools
    private bool introShown, finalShown, slidingLeft, makeDrill, imagesActivated = false;
    //timers
    private float toastTimer;
    private float jumpPhaseTimer;
    private float panelSlidingTimer;

    public bool SucceededDrill { get; set; }
    private List<GameObject> rocks = new List<GameObject>();
    public DrillingGameState State { get { return state; } set { state = value; } }
    public DrillingDirection DrillDirection { get { return drillDir; } set { drillDir = value; } }
    public DrillingDirection PrevDrillDirection { get { return prevDrillDir; } set { prevDrillDir = value; } }
    public void MakeDrill(bool value) { makeDrill = value; }
    public UnityEngine.UI.Image Drill { get { return drill; } }
    public float DiamondValue { get { return diamondValue; } }
    public UnityEngine.UI.Image GlobeDrillGroundIcon { get { return globeDrillGroundIcon; } }
    public UnityEngine.UI.Image GlobeDrillPipeIcon { get { return globeDrillPipeIcon; } }
    public UnityEngine.UI.Image BgActive { get { return bgActive; } }
    public UnityEngine.UI.Image MainPanel { get { return mainPanel; } }
    public Animator Animator { get { return animator; } }
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
        if (mainPanel) mainPanel.rectTransform.anchoredPosition = new Vector3(0, -(Screen.height / 2) - 420, 0);
        if (drill) initDrillPos = drill.rectTransform.anchoredPosition;
        if (globeDrillPipeIcon && globeDrillGroundIcon) globeDrillPipeIcon.transform.SetSiblingIndex(globeDrillGroundIcon.transform.GetSiblingIndex() - 1);
    }

    public void StartGame(Drillspot drillspot, float difficulty)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin(difficulty);
        state = DrillingGameState.ACTIVATION;
        imagesActivated = true;
        introShown = true;
        drill.transform.SetAsLastSibling();
        if (animator) animator.SetBool("isSlidingLeft", false);
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0,100,0), panelSlidingTime).setEase(LeanTweenType.easeOutQuad);
    }

    private void generateMap()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < rows.Length-1; j++)
            {
                //instantiateGroundTile(columns[i], rows[j]);
                //float rand = Random.Range(0f, 1f);
                //if (rand > 0.2f) instantiateGroundTile(columns[i], rows[j]);
                //else instantiateRock(columns[i], rows[j]); //try rock
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
            generateMap();
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
                endOkToast.transform.SetAsLastSibling();
                if (toastTimer < 0.0f) fireToast(true);
            }
            else
            {
                endFailToast.gameObject.SetActive(true);
                endFailToast.transform.SetAsLastSibling();
                if (toastTimer < 0.0f) fireToast(false);
            }
        }
    }

    private void fireToast(bool gameSucceeded)
    {
        finalShown = true;
        toastTimer = toastMessageTime;
        endOkToast.gameObject.SetActive(false);
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
            if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1])
                drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
            else
            {
                targetColumn++;
                prevDrillDir = DrillingDirection.DOWN;
            }
        }
        else if (prevDrillDir == DrillingDirection.LEFT)
        {
            if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
            else
            {
                targetColumn--;
                prevDrillDir = DrillingDirection.DOWN;
            }
        }
        else drill.transform.Translate(0, -1.0f * drillSpeed * Time.deltaTime, 0); //drill down

        if (targetRow < rows.Length - 1 && drill.rectTransform.anchoredPosition.y <= rows[targetRow + 1]) targetRow++;
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
        else drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left

        if (targetColumn > 0)
        {
            if (drill.rectTransform.anchoredPosition.x <= columns[targetColumn - 1]) targetColumn -= 1;
        }
        else
        {
            SucceededDrill = false;
            state = DrillingGameState.STARTSTOPTOAST;
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
        else drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right

        //check against right wall and game over if bump
        if (targetColumn < columns.Length - 1)
        {
            if (drill.rectTransform.anchoredPosition.x >= columns[targetColumn + 1]) targetColumn += 1;
        }
        else
        {
            SucceededDrill = false;
            state = DrillingGameState.STARTSTOPTOAST;
        }
    }

    private void drillUp()
    {
        if (prevDrillDir == DrillingDirection.RIGHT)
        {
            if (targetColumn < columns.Length-1 && drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1])
                drill.transform.Translate(1 * drillSpeed * Time.deltaTime, 0, 0); //drill right
            else
            {
                targetColumn++;
                prevDrillDir = DrillingDirection.UP;
            }
        }
        else if (prevDrillDir == DrillingDirection.LEFT)
        {
            if (targetColumn > 0 && drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1])
                drill.transform.Translate(-1 * drillSpeed * Time.deltaTime, 0, 0); //drill left
            else
            {
                targetColumn--;
                prevDrillDir = DrillingDirection.UP;
            }
        }
        else drill.transform.Translate(0, 1.0f * drillSpeed * Time.deltaTime, 0); //drill down

        if (targetRow > 0)
        {
            if (drill.rectTransform.anchoredPosition.y >= rows[targetRow - 1]) targetRow--;
        }
        else
        {
            SucceededDrill = false;
            state = DrillingGameState.STARTSTOPTOAST;
        }
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
        if(drill) updateState();
        if (IsRunning && Timeleft <= 0.5f) End(false);
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && state != DrillingGameState.INACTIVE)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
        }
        Debug.Log("row: " + targetRow + " | column: " + targetColumn);
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
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
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
        foreach (GameObject rock in rocks) Destroy(rock);
        drill.rectTransform.anchoredPosition = initDrillPos;
        rocks.Clear();
        LeanTween.move(mainPanel.gameObject.GetComponent<RectTransform>(), new Vector3(0, -(Screen.height / 2) - 420, 0), panelSlidingTime / 2);
    }

    private void instantiateRock(int x, int y)
    {
        GameObject rock = Instantiate(rockPrefab) as GameObject;
        rock.transform.SetParent(mainPanel.transform, false);
        rock.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        rock.gameObject.SetActive(true);
        rocks.Add(rock);

        LeanTween.scale(rock.GetComponent<RectTransform>(), rock.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateDiamond(int x, int y)
    {
        GameObject diamond = Instantiate(diamondPrefab) as GameObject;
        diamond.transform.SetParent(mainPanel.transform, false);
        diamond.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        diamond.gameObject.SetActive(true);
        rocks.Add(diamond);

        LeanTween.scale(diamond.GetComponent<RectTransform>(), diamond.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateGroundTile(int x, int y)
    {
        GameObject groundTile = Instantiate(groundTilePrefab) as GameObject;
        groundTile.transform.SetParent(mainPanel.transform, false);
        groundTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        groundTile.gameObject.SetActive(true);
        rocks.Add(groundTile);

        LeanTween.scale(groundTile.GetComponent<RectTransform>(), groundTile.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateCable(int x, int y)
    {
        GameObject cable = Instantiate(cablePrefab) as GameObject;
        cable.transform.SetParent(mainPanel.transform, false);
        cable.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        cable.gameObject.SetActive(true);
        rocks.Add(cable);

        LeanTween.scale(cable.GetComponent<RectTransform>(), cable.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }

    private void instantiateBomb(int x, int y)
    {
        GameObject bomb = Instantiate(cablePrefab) as GameObject;
        bomb.transform.SetParent(mainPanel.transform, false);
        bomb.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        bomb.gameObject.SetActive(true);
        rocks.Add(bomb);

        LeanTween.scale(bomb.GetComponent<RectTransform>(), bomb.GetComponent<RectTransform>().localScale * 1.2f, 1f)
            .setEase(LeanTweenType.punch);
    }
}
