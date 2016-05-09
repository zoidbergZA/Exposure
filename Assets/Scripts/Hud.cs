using UnityEngine;
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
        if (GUI.Button(new Rect(10,Screen.height - 65, 55, 55), "scan"))
        {

        }
    }

    private void ShowDebug()
    {
        GUILayout.Label("score: " + GameManager.Instance.Player.Score + "/" + "100");
    }
}
