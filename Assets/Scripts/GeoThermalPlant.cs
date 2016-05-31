using UnityEngine;
using System.Collections;

public class GeoThermalPlant : Connectable
{
    public override void CheckConnectable(Vector3 location)
    {
        IsConnectable = false;
    }

    public override void Highlight(bool hightlight)
    {
//        throw new System.NotImplementedException();
    }

    public override void OnConnected()
    {
//        throw new System.NotImplementedException();
    }
}
