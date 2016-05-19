﻿using UnityEngine;
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

    void OnGUI()
    {
        float wobbleValue = 7f * GameManager.Instance.Hud.WobbleValue;

//        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
//        if (GUI.Button(new Rect(screenPos.x - 20 - wobbleValue/2, Screen.height - screenPos.y - 20 - wobbleValue/2, 40 + wobbleValue, 40 + wobbleValue), connectIcon, ""))
//        {
//            
//        }

        if (IsConnectable && GameManager.Instance.Player.PlayerState == Player.PlayerStates.BuildGrid)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

            if (GUI.Button(new Rect(screenPos.x - 20 - wobbleValue / 2, Screen.height - screenPos.y - 20 - wobbleValue / 2, 40 + wobbleValue, 40 + wobbleValue), connectIcon, ""))
            {
                OnConnected();
                GameManager.Instance.GridBuilder.MakeConnection(this);
            }
        }
    }
}
