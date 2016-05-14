using UnityEngine;
using System.Collections;

public class DrillingGame : Minigame
{
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private UnityEngine.UI.Image bgActive;
    [SerializeField] private UnityEngine.UI.Image bgInactive;
    [SerializeField] private UnityEngine.UI.Image drill;
    private Drillspot drillspot;
    private enum DrillingGameState { INACTIVE, SLIDING, DRILLING, SUCCESS }
    private DrillingGameState state;
    private bool makeDrill = false;
    private Vector3 initDrillPos;

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning) return;
        this.drillspot = drillspot;
        Begin();
        state = DrillingGameState.SLIDING;
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
        }
        else
        {
            state = DrillingGameState.INACTIVE;
            drill.transform.position = initDrillPos;
            End(true);
        }
    }

    private void handleSlidingState()
    {
        activateImages(true);
        updateDrillMovement();
    }

    private void updateDrillMovement()
    {
        if (!makeDrill)
        {
            if(drill) drill.gameObject.transform.Translate(Mathf.Sin(Time.time), 0, 0);
            if (Input.GetKeyDown(KeyCode.Space)) makeDrill = true;
        }
        else state = DrillingGameState.DRILLING;
    }

    private void handleInactiveState()
    {
        activateImages(false);
    }

    private void handleSuccessState()
    {
        //End(true);
    }

    void Start()
    {
        activateImages(false);
        if (drill) initDrillPos = drill.transform.position;
    }

    public override void Update()
    {
        base.Update();
        updateState();

        //todo: minigame logic here, auto-win close to timeOut for now
        if (IsRunning && Timeleft <= 0.5f) End(false);
    }

    public override void End(bool succeeded)
    {
        base.End(succeeded);

        if (succeeded)
        {
            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, drillspot.transform.position, drillspot.transform.rotation) as GeoThermalPlant;
            Destroy(drillspot.gameObject);
            
            makeDrill = false;
            GameManager.Instance.Player.StartBuildMinigame(plant);
        }
        else
        {
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
            makeDrill = false;
        }
    }

    private void activateImages(bool activate)
    {
        if(activate)
        {
            if (bgActive) bgActive.gameObject.SetActive(true);
            if (bgInactive) bgInactive.gameObject.SetActive(false);
            if (drill) drill.gameObject.SetActive(true);
        } else
        {
            if (bgActive) bgActive.gameObject.SetActive(false);
            if (bgInactive) bgInactive.gameObject.SetActive(true);
            if (drill) drill.gameObject.SetActive(false);
        }
    }
}
