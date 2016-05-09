using UnityEngine;
using System.Collections;

public abstract class Connectable : Placable
{
    public Transform connectionRef;

    public bool IsConnectable { get; protected set; }

    public abstract void CheckConnectable(Vector3 location);
    public abstract void Highlight(bool hightlight);
    public abstract void Connect();
}
