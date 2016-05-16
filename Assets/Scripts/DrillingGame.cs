using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrillingGame : Minigame
{
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image bgInactive;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Text timer;
    [SerializeField] private UnityEngine.UI.Image startToast;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image endFailToast;
    [SerializeField] private UnityEngine.UI.Text startToastTimer;
    [SerializeField] private UnityEngine.UI.Text endOkToastTimer;
    [SerializeField] private UnityEngine.UI.Text endFailToastTimer;
    [SerializeField] private int[] columns;
    [SerializeField] private int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float heatValue;
    [SerializeField] private float RockDiamondRatio;
    [SerializeField] private float toastMessageTime = 3.0f;
    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS, STARTSTOPTOAST }
    private DrillingGameState state;
    private bool makeDrill = false;
    private Vector3 initDrillPos;
    private int targetColumn;
    private int targetRow;
    private float toastTimer;
    private bool slidingLeft = false;
    private bool introShown, finalShown = false;
    private List<GameObject> rocks = new List<GameObject>();
    public DrillingGameState State { get { return state; } set { state = value; } }
    public void SetMakeDrill(bool value) { makeDrill = value; }
    public UnityEngine.UI.Image GetDrill { get { return drill; } }

    void Start()
    {
        activateImages(false);
        if (drill) initDrillPos = drill.transform.position;
        targetColumn = 0;
        toastTimer = toastMessageTime;
    }

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin();
        state = DrillingGameState.STARTSTOPTOAST;
    }

    private void generateMap()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < rows.Length; j++)
            {
                float temp = Random.Range(0.01f, 1.0f);
                if(temp <= heatValue)
                {
                    float temp2 = Random.Range(0.01f, 1.0f);
                    if (temp2 <= RockDiamondRatio)
                    {
                        GameObject rock = Instantiate(rockPrefab) as GameObject;
                        rock.transform.SetParent(canvas.transform, false);
                        rock.GetComponent<RectTransform>().anchoredPosition = new Vector3(columns[i], rows[j]);
                        rock.gameObject.SetActive(true);
                        rocks.Add(rock);
                    }
                    else
                    {
                        GameObject diamond = Instantiate(diamondPrefab) as GameObject;
                        diamond.transform.SetParent(canvas.transform, false);
                        diamond.GetComponent<RectTransform>().anchoredPosition = new Vector3(columns[i], rows[j]);
                        diamond.gameObject.SetActive(true);
                        rocks.Add(diamond);
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
        }
    }

    private void handleStartStopState()
    {
        if(!introShown && !finalShown)
        {
            toastTimer -= Time.deltaTime;
            startToast.gameObject.SetActive(true);
            startToastTimer.text = "Game starts\nin: " + ((int)toastTimer).ToString();
            if (toastTimer < 0.0f)
            {
                introShown = true;
                toastTimer = toastMessageTime;
                startToast.gameObject.SetActive(false);
                state = DrillingGameState.SLIDING;
                generateMap();
            }
        }
        else if(introShown && !finalShown)
        {
            state = DrillingGameState.INACTIVE;
        }
    }

    private void handleDrillingState()
    {
        if (drill && drill.transform.position.y > initDrillPos.y - 350)
        {
            drill.transform.Translate(0, -1.0f, 0);
            if (targetRow < rows.Length - 1)
            {
                if (drill.rectTransform.anchoredPosition.y == rows[targetRow + 1] && targetRow < rows.Length - 1) targetRow++;
            }
        }
        else
        {
            drill.transform.position = initDrillPos;
            state = DrillingGameState.SUCCESS;
        }
    }

    public void MoveRight()
    {
        if (targetColumn < columns.Length - 1)
        {
            while (drill.rectTransform.anchoredPosition.x <= columns[targetColumn + 1]) drill.transform.Translate(new Vector3(1, 0, 0));
            targetColumn += 1;
        }
    }

    public void MoveLeft()
    {
        if (targetColumn > 0)
        {
            while (drill.rectTransform.anchoredPosition.x >= columns[targetColumn - 1]) drill.transform.Translate(new Vector3(-1, 0, 0));
            targetColumn -= 1;
        }
    }

    private void handleSlidingState()
    {
        activateImages(true);
        if(drill) updateSlidingMovement();
    }

    private void updateSlidingMovement()
    {
        if (!makeDrill)
        {
            if (slidingLeft == false)
            {
                drill.transform.Translate(new Vector3(1, 0, 0));
                if (targetColumn < columns.Length - 1)
                {
                    if (drill.rectTransform.anchoredPosition.x == columns[targetColumn + 1]) targetColumn += 1;
                }
                else slidingLeft = true;
            }
            else
            {
                drill.transform.Translate(new Vector3(-1, 0, 0));
                if (targetColumn > 0)
                {
                    if (drill.rectTransform.anchoredPosition.x == columns[targetColumn - 1]) targetColumn -= 1;
                }
                else slidingLeft = false;
            }
        }
        else
        {
            if (slidingLeft == false)
            {
                if (drill.rectTransform.anchoredPosition.x < columns[targetColumn + 1]) drill.transform.Translate(new Vector3(1, 0, 0));
                else
                {
                    state = DrillingGameState.DRILLING;
                    if (targetColumn < columns.Length - 1) targetColumn += 1;
                }
            }
            else
            {
                if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1]) drill.transform.Translate(new Vector3(-1, 0, 0));
                else
                {
                    state = DrillingGameState.DRILLING;
                    if (targetColumn > 0) targetColumn -= 1;
                }
            }
        }
    }

    private void handleInactiveState()
    {
        activateImages(false);
    }

    private void handleSuccessState()
    {
        state = DrillingGameState.INACTIVE;
        End(true);
    }

    public override void Update()
    {
        base.Update();
        updateState();
        if (IsRunning && Timeleft <= 0.5f) End(false);
        if (GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal && state != DrillingGameState.INACTIVE)
        {
            state = DrillingGameState.INACTIVE;
            End(false);
        }
        if (state != DrillingGameState.INACTIVE) timer.text = ((int)Timeleft).ToString();
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            Destroy(drillspot.gameObject);
            GameManager.Instance.Player.StartBuildMinigame(plant);
        }
        else GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        
        resetGameGuts();
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (bgActive) bgActive.gameObject.SetActive(true);
            if (bgInactive) bgInactive.gameObject.SetActive(false);
            if (drill) drill.gameObject.SetActive(true);
            if (timer) timer.gameObject.SetActive(true);
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
            if (bgInactive) bgInactive.gameObject.SetActive(true);
            if (drill) drill.gameObject.SetActive(false);
            if (timer) timer.gameObject.SetActive(false);
            if (startToast) startToast.gameObject.SetActive(false);
            if (endOkToast) endOkToast.gameObject.SetActive(false);
            if (endFailToast) endFailToast.gameObject.SetActive(false);
        }
    }

    private void resetGameGuts()
    {
        makeDrill = false;
        slidingLeft = false;
        introShown = false;
        finalShown = false;
        targetColumn = 0;
        foreach (GameObject rock in rocks) Destroy(rock);
        drill.transform.position = initDrillPos;
    }
}
