using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pylon : Connectable
{
    public enum States
    {
        Ready,
        Preview,
        Built
    }
    
    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;

    public States State { get; private set; }

    void Awake()
    {
        Reset();
    }

    public void ShowPreview()
    {
        if (State != States.Ready)
            return;

        State = States.Preview;
        PlacerModel.SetActive(true);
    }

    public void Build()
    {
        if (State == States.Built)
            return;

        State = States.Built;
        
        PlacerModel.SetActive(false);
        BuiltModel.SetActive(true);
    }

    public void Reset()
    {
        State = States.Ready;
        BuiltModel.SetActive(false);
        PlacerModel.SetActive(false);
    }
}
