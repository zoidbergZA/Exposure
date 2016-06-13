﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class GeoThermalPlant : Connectable
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject Model;

    public States State { get; private set; }
    public PuzzlePath PuzzlePath { get; private set; }

    void Awake()
    {
//        Reset();
    }

    public override void OnConnected()
    {
//        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        base.Reset();

        State = States.Ready;
        Model.SetActive(false);
    }

    public void SetMyPuzzlePath(PuzzlePath puzzlePath)
    {
        PuzzlePath = puzzlePath;
    }

    public void Build()
    {
        if (State != States.Ready)
            return;

        State = States.Built;
    }

    public void ShowPreview(bool show)
    {
        if (State != States.Ready)
            return;

        if (show)
        {
            Model.SetActive(true);
        }
        else
        {
            Model.SetActive(false);
        }
    }
}
