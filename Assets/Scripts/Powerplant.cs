﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(LineRenderer))]
public class Powerplant : Placable
{
    private Seeker seeker;
    private LineRenderer lineRenderer;
    private Path path;
    private City connectedCity;

    public override void Awake()
    {
        base.Awake();

        seeker = GetComponent<Seeker>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Start()
    {
        base.Start();

        connectedCity = FindClosestCity();
        seeker.StartPath(transform.position, connectedCity.transform.position, OnPathComplete);

        connectedCity.DisableChimney();
    }

    public void OnPathComplete(Path p)
    {
//        Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);

        path = p;
        List<Vector3> linePath = new List<Vector3>();

        linePath.Add(transform.position);

        for (int i = 0; i < path.path.Count; i++)
        {
            linePath.Add(path.vectorPath[i]);
        }

        linePath.Add(FindClosestCity().transform.position);

        lineRenderer.SetVertexCount(linePath.Count);
        lineRenderer.SetPositions(linePath.ToArray());
    }

    protected City FindClosestCity()
    {
        City[] cities = FindObjectsOfType<City>();
        City closest = cities[0];
        float distance = Vector3.Distance(transform.position, cities[0].transform.position);

        if (cities.Length > 1)
        {
            for (int i = 1; i < cities.Length; i++)
            {
                float d = Vector3.Distance(transform.position, cities[i].transform.position);

                if (d < distance)
                {
                    closest = cities[i];
                    distance = d;
                }
            }
        }

        return closest;
    }
}
