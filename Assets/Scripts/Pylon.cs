using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pylon : Connectable
{
    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;

    void Awake()
    {
        Reset();
    }

    public override void OnConnected()
    {
        Build();
    }

    public override void ShowPreview()
    {
        base.ShowPreview();

        PlacerModel.SetActive(true);
    }

    public void Build()
    {
        if (ConnectionState == ConnectionStates.Built)
            return;

        ConnectionState = ConnectionStates.Built;
        
        PlacerModel.SetActive(false);
        BuiltModel.SetActive(true);
    }

    public override void Reset()
    {
        ConnectionState = ConnectionStates.Hidden;
        BuiltModel.SetActive(false);
        PlacerModel.SetActive(false);
    }
}
