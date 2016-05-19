using UnityEngine;
using System.Collections;

public abstract class Connectable : Placable
{
    public Transform connectionRef;

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
