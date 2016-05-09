using UnityEngine;
using System.Collections;

public class GridBuilder : MonoBehaviour
{
    [SerializeField] private float gridTime = 15f;
    [SerializeField] private int maxPylons = 4;

    public float GridTimeLeft { get; set; }
    public GeoThermalPlant StartPlant { get; private set; }

    public void StartBuild(GeoThermalPlant from)
    {
        GridTimeLeft = gridTime;
        StartPlant = from;
    }

    public void EndBuild()
    {
        Destroy(StartPlant.gameObject);
    }

    private void Reset()
    {
        StartPlant = null;
    }
}
