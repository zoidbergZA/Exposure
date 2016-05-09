using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class Pylon : Connectable
{
    public enum States
    {
        Ready,
        Built
    }

    [SerializeField] private GameObject PlacerModel;
    [SerializeField] private GameObject BuiltModel;
    private List<Pylon> Connections;

    public States State { get; private set; }

    public override void Awake()
    {
        base.Awake();

        Connections = new List<Pylon>();
    }

    public void AddConnection(Pylon other)
    {
        if (other.gameObject == this || Connections.Contains(other))
            return;

        Connections.Add(other);
    }

    public void Build()
    {
        if (State != States.Ready)
            return;

        State = States.Built;
        GameManager.Instance.Player.AddToConnectedList(this);
        GameManager.Instance.GridBuilder.StartPlant.SpanToPoint(connectionRef.position);
        GameManager.Instance.Director.SetTarget(transform);
        GameManager.Instance.Player.RefreshConnectables(transform.position);

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

        if (dist <= GameManager.nodeDistance && State == States.Ready)
        {
            IsConnectable = true;
            Highlight(true);
        }
        else
        {
            IsConnectable = false;
            Highlight(false);
        }
    }

    public override void Highlight(bool highlight)
    {
        if (State != States.Ready)
            return;

        if (highlight)
            PlacerModel.GetComponent<MeshRenderer>().material.color = Color.green;
        else
            PlacerModel.GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public override void Connect()
    {
        Build();
    }
}
