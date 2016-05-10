﻿using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour
{
    void OnGUI()
    {
        ShowScanButton();
        ShowDebug();
    }

    private void ShowScanButton()
    {
        if (GameManager.Instance.Scanner.IsReady)
        {
            if (GUI.Button(new Rect(10, Screen.height - 65, 55, 55), "scan"))
            {
                GameManager.Instance.Scanner.Scan();
            }
        }
        else
        {
            GUI.Label(new Rect(10, Screen.height - 65, 55, 55), GameManager.Instance.Scanner.Cooldown.ToString("F2"));
        }
    }

    private void ShowDebug()
    {
        GUILayout.Label("score: " + GameManager.Instance.Player.Score + "/" + "100");
    }
}
