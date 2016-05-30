﻿using UnityEngine;
using System.Collections;

public class GeoThermalPlant : Powerplant
{
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
    }

    public void SpanToPoint(Vector3 point)
    {
        for (int i = spanIndex; i < 10; i++)
        {
            point = transform.InverseTransformPoint(point); // world point to local space
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
            positions[i] = Vector3.zero; // transform.position;
        }

        powerLineRenderer.SetPositions(positions);
        spanIndex = 1;
    }
}
