﻿using UnityEngine;
using System.Collections;

public abstract class Connectable : Placable
{
    public Transform connectionRef;

    public bool IsConnectable { get; protected set; }

    public abstract void CheckConnectable(Vector3 location);
    public abstract void Highlight(bool hightlight);
    public abstract void OnConnected();

    void OnGUI()
    {
        if (IsConnectable && GameManager.Instance.Player.PlayerState == Player.PlayerStates.BuildGrid)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            if (GUI.Button(new Rect(screenPos.x - 20, Screen.height - screenPos.y - 20, 40, 40), "build"))
            {
                OnConnected();
                GameManager.Instance.GridBuilder.MakeConnection(this);
            }
        }
    }
}
