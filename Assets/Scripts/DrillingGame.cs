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

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning)
            return;

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
            default:
                handleInactiveState();
                break;
        }
    }

    private void handleDrillingState()
    {

    }

    private void handleSlidingState()
    {
        if (bgActive) bgActive.gameObject.SetActive(true);
        if (bgInactive) bgInactive.gameObject.SetActive(false);
        if (drill) drill.gameObject.SetActive(true);
    }

    private void handleInactiveState()
    {
        End(false);
    }

    private void handleSuccessState()
    {
        End(true);
    }

    void Start()
    {
        state = DrillingGameState.INACTIVE;
        if (bgActive) bgActive.gameObject.SetActive(false);
        if (bgInactive) bgInactive.gameObject.SetActive(true);
        if (drill) drill.gameObject.SetActive(false);
    }

    public override void Update()
    {
        base.Update();
        updateState();

        //todo: minigame logic here, auto-win close to timeOut for now
        if (IsRunning && Timeleft <= 0.5f) End(false);

        //if (drill) drill.gameObject.transform.Translate(new Vector3(Mathf.Sin(Time.time)*2, 0, 0));
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
        else
        {
            GameManager.Instance.Player.GoToNormalState(GameManager.Instance.PlanetTransform);
        }
    }
}
