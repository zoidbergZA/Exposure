using UnityEngine;
using System.Collections;

public class GeoThermalPlant : Powerplant
{
    public float Output { get; protected set; } // power output from 0 to 1.

    private LineRenderer cableRenderer;
    private int spanIndex = 1; 

    public override void Awake()
    {
        base.Awake();
        
        InitCableRenderer();
    }

    public override void Start()
    {
        base.Start();

        Output = Random.Range(0.2f, 1.0f);

        GameManager.Instance.AddPowerOutput(Output);
    }

    public void SpanToPoint(Vector3 point)
    {
        for (int i = spanIndex; i < 10; i++)
        {
            powerLineRenderer.SetPosition(i, point);
        }

//        powerLineRenderer.SetPosition(spanIndex, point);
        spanIndex++;
    }

    public void InitCableRenderer()
    {
        Vector3[] positions = new Vector3[10];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.position;
        }

        powerLineRenderer.SetPositions(positions);
        spanIndex = 1;
    }
}
