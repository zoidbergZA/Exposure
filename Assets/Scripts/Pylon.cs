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

    [SerializeField] private float chargeBoost = 7f;
    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;

    public States State { get; private set; }

    void Awake()
    {
        Reset();
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
        GameManager.Instance.Scanner.AddCharge(chargeBoost);
        
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
