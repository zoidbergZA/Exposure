using UnityEngine;
using System.Collections;

public class DrillingGame : Minigame
{
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image bgInactive;
    [SerializeField] private UnityEngine.UI.Image drill;
    [SerializeField] private UnityEngine.UI.Image rock;
    [SerializeField] private int[] columns;
    [SerializeField] private int[] rows;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float heatValue;
    private Drillspot drillspot;
    public enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS }
    private DrillingGameState state;
    private bool makeDrill = false;
    private Vector3 initDrillPos;
    private int targetColumn;
    private int targetRow;
    private bool slidingLeft = false;
    public DrillingGameState GetState { get { return state; } }
    public void SetMakeDrill(bool value) { makeDrill = value; }
    public UnityEngine.UI.Image GetDrill { get { return drill; } }

    void Start()
    {
        activateImages(false);
        if (drill) initDrillPos = drill.transform.position;
        targetColumn = 0;
    }

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin();
        state = DrillingGameState.SLIDING;
        generateMap();
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
                    GameObject rock2 = Instantiate(rockPrefab) as GameObject;
                    rock2.transform.SetParent(canvas.transform, false);
                    rock2.GetComponent<RectTransform>().anchoredPosition = new Vector3(columns[i], rows[j]);
                    rock2.gameObject.SetActive(true);
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
                    targetRow = 0;
                }
            }
            else
            {
                if (drill.rectTransform.anchoredPosition.x > columns[targetColumn - 1]) drill.transform.Translate(new Vector3(-1, 0, 0));
                else
                {
                    state = DrillingGameState.DRILLING;
                    if (targetColumn > 0) targetColumn -= 1;
                    targetRow = 0;
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
            slidingLeft = false;
        }
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            Destroy(drillspot.gameObject);
            
            makeDrill = false;
            slidingLeft = false;
            GameManager.Instance.Player.StartBuildMinigame(plant);
        }
        else
        {
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
            makeDrill = false;
            slidingLeft = false;
        }
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (bgActive) bgActive.gameObject.SetActive(true);
            if (bgInactive) bgInactive.gameObject.SetActive(false);
            if (drill) drill.gameObject.SetActive(true);
            if (rock) rock.gameObject.SetActive(true);
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
            if (bgInactive) bgInactive.gameObject.SetActive(true);
            if (drill) drill.gameObject.SetActive(false);
            if (rock) rock.gameObject.SetActive(false);
            targetColumn = 0;
        }
    }
}
