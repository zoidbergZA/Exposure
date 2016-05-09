using UnityEngine;
using System.Collections;

public class Drillspot : Placable
{
    public enum DrillStates
    {
        Busy,
        Succeeded,
        Failed
    }

    [SerializeField] private GeoThermalPlant geoThermalPlantPrefab;
    [SerializeField] private GameObject model;
    [SerializeField] private float drillTime = 0.8f;

    private float timer;

    public DrillStates DrillState { get; private set; }

    public override void Awake()
    {
        base.Awake();

        timer = drillTime;
    }

    public override void Update()
    {
        base.Update();

        if (DrillState == DrillStates.Busy)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                CompleteDrill();        
            }
        }
    }

    private void CompleteDrill()
    {
        float rand = Random.Range(0f, 1f);

        if (rand < 0.5f)
        {
            DrillState = DrillStates.Succeeded;
            GetComponentInChildren<MeshRenderer>().material.color = Color.green;

            GeoThermalPlant plant = Instantiate(geoThermalPlantPrefab, transform.position, transform.rotation) as GeoThermalPlant;

            GameManager.Instance.Player.GoToBuildState(plant);

            Destroy(gameObject);
        }
        else
        {
            DrillState = DrillStates.Failed;
            GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }
}
