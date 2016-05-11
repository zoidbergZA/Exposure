using UnityEngine;
using System.Collections;

public class DrillingGame : Minigame
{
    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    private Drillspot drillspot;

    public void StartGame(Drillspot drillspot)
    {
        if (IsRunning)
            return;

        this.drillspot = drillspot;

        Begin();
    }

    public override void Update()
    {
        base.Update();

        //todo: minigame logic here, auto-win close to timeOut for now
        if (IsRunning && Timeleft <= 0.5f)
            End(true);
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
