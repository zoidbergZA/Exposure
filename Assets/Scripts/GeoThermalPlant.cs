using UnityEngine;
using System.Collections;

public class GeoThermalPlant : Powerplant
{
    public float Output { get; protected set; } // power output from 0 to 1.

    public override void Start()
    {
        base.Start();

        Output = Random.Range(0.2f, 1.0f);

        GameManager.Instance.AddPowerOutput(Output);
    }
}
