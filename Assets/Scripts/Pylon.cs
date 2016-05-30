using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

public class Pylon : Connectable
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;
    
//    private List<Pylon> Connections;

    public States State { get; private set; }

    public override void Awake()
    {
        base.Awake();

//        Connections = new List<Pylon>();
        BuiltModel.SetActive(false);
//        PlacerModel.SetActive(false); // commented for testing
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

    public override void CheckConnectable(Vector3 location)
    {
        float dist = Vector3.Distance(transform.position, location);
        
        if (dist <= GameManager.Instance.GridBuilder.PylonSeparation * 1.3f && State != States.Built && !GameManager.Instance.GridBuilder.ConnectedList.Contains(this))
        {
            IsConnectable = true;
//            Highlight(true);
        }
        else
        {
            IsConnectable = false;
//            Highlight(false);
        }
    }

    public override void Highlight(bool highlight)
    {
        if (State != States.Ready)
            return;

        IsConnectable = highlight;
    }

    public void ShowPlacer(bool show)
    {
        PlacerModel.SetActive(show);
    }

    public override void OnConnected()
    {
        Build();
    }
}
