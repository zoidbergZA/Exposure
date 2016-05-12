﻿using UnityEngine;
using System.Collections;

public class Drillspot : Placable
{
    public enum DrillStates
    {
        Busy,
        Succeeded,
        Failed
    }

    [SerializeField] private GameObject model;
    [SerializeField] private float drillTime = 0.8f;

    private float timer;

    public DrillStates DrillState { get; private set; }

    public override void Awake()
    {
        base.Awake();

        timer = drillTime;
    }

    public override void Start()
    {
        base.Start();

        //todo: do small scan around drillspot
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
        //todo: check UV map if drill will succeed

        float rand = Random.Range(0f, 1f);

        if (rand < 1f)
        {
            DrillState = DrillStates.Succeeded;
            GameManager.Instance.Player.StartDrillMinigame(this);
        }
        else
        {
            DrillState = DrillStates.Failed;
            GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }
}
