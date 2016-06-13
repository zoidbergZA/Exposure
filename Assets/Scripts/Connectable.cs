using UnityEngine;
using System.Collections;

public abstract class Connectable : Placable
{
    public Transform connectionRef;
    public void MakeConnection(Connectable other)
    {
        float dist = Vector3.Distance(connectionRef.position, other.connectionRef.position);

        GameObject pipeModel = Instantiate(GameManager.Instance.PipePrefab);

        pipeModel.transform.SetParent(transform);
        pipeModel.transform.localScale = new Vector3(1, 1, dist*0.5f);
        pipeModel.transform.position = connectionRef.position;
        pipeModel.transform.LookAt(other.connectionRef);
    }
}
