﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject radar;
    [SerializeField] private float scanTime;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private Powerplant PowerplantPrefab;
    [SerializeField] private Drillspot DrillspotPrefab;

    public float Score { get; private set; }
    public bool Scanning { get; private set; }
    public float LastScan { get; private set; }

    void Update()
    {
        if (Scanning && Time.time > LastScan + scanTime)
            Scanning = false;

        if (Input.GetMouseButtonDown(1) && !Scanning)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Scan(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayMask))
            {
                Drill(hit.point, hit.normal);
            }
        }

        if (Scanning)
        {
            radar.SetActive(true);
        }
        else
        {
            radar.SetActive(false);
        }
    }

    public void ScorePoints(float amount)
    {
        Score += amount;
    }

    private void Drill(Vector3 location, Vector3 normal)
    {
        Drillspot drillspot = Instantiate(DrillspotPrefab, location, Quaternion.identity) as Drillspot;
        drillspot.Orientate(normal);
    }

    private void Scan(Vector3 location)
    {
        LastScan = Time.time;
        Scanning = true;
        radar.transform.position = location;
    }
}
