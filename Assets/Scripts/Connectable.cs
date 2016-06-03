using UnityEngine;
using System.Collections;

public abstract class Connectable : Placable
{
    public Transform connectionRef;
//    [SerializeField] private GameObject PipeModel;

    [SerializeField] protected float separationMultiplier = 1f;
    [SerializeField] private Texture2D connectIcon;

    public bool IsConnectable { get; protected set; }

    public abstract void CheckConnectable(Vector3 location);
    public abstract void Highlight(bool hightlight);
    public abstract void OnConnected();

    public void TurnOff()
    {
        IsConnectable = false;
        Highlight(false);
    }

    public void AddConnection(Connectable other)
    {
        float dist = Vector3.Distance(connectionRef.position, other.connectionRef.position);
//        Debug.Log(dist);

        GameObject pipeModel = Instantiate(GameManager.Instance.PipePrefab);

        pipeModel.transform.SetParent(transform);
        pipeModel.transform.localScale = new Vector3(1, 1, dist*0.5f);
        pipeModel.transform.position = connectionRef.position;
        pipeModel.transform.LookAt(other.connectionRef);

        //        if (other.gameObject == this || Connections.Contains(other))
        //            return;
        //
        //        Connections.Add(other);
    }

    private void ClickCallback()
    {
        OnConnected();
        GameManager.Instance.GridBuilder.MakeConnection(this);
    }

    void OnGUI()
    {
        if (IsConnectable && GameManager.Instance.Player.PlayerState == Player.PlayerStates.BuildGrid)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            GameManager.Instance.Hud.ShowWorldSpaceButton(connectIcon, screenPos, ClickCallback);
        }
    }
}
