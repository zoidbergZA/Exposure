using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pylon : MonoBehaviour
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;

    public States State { get; private set; }

    void Awake()
    {
        BuiltModel.SetActive(false);
        PlacerModel.SetActive(false);
    }

    public void ShowPreview(bool show)
    {
        if (State != States.Ready)
            return;

        if (show)
        {
            PlacerModel.SetActive(true);
        }
        else
        {
            PlacerModel.SetActive(false);
        }
    }

    public void Build()
    {
        if (State != States.Ready)
            return;

        State = States.Built;
        
        PlacerModel.SetActive(false);
        BuiltModel.SetActive(true);
    }

    public void Reset()
    {
        State = States.Ready;
        PlacerModel.SetActive(true);
        BuiltModel.SetActive(false);
    }
}
