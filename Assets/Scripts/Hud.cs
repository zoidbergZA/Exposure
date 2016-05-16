using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour
{
    void OnGUI()
    {
        ShowScanButton();
        ShowDebug();
        ShowDrillButton();
        ShowSteerButtons();
    }

    private void ShowScanButton()
    {
        if (GameManager.Instance.Scanner.IsReady && GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
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

    private void ShowDrillButton()
    {
        if(GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (GUI.Button(new Rect(10, Screen.height - 65, 55, 55), "Drill!"))
            {
                GameManager.Instance.DrillingGame.SetMakeDrill(true);
            }
        }
    }

    private void ShowSteerButtons()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (GUI.Button(new Rect(10, Screen.height - 65, 55, 55), "LEFT"))
            {
                GameManager.Instance.DrillingGame.MoveLeft();
            }

            if (GUI.Button(new Rect(100, Screen.height - 65, 55, 55), "RIGHT"))
            {
                GameManager.Instance.DrillingGame.MoveRight();
            }
        }
    }

    private void ShowDebug()
    {
        //time left
        string timeText = "";
        if (GameManager.Instance.RoundStarted)
            GUILayout.Label("time left: " + GameManager.Instance.TimeLeft.ToString("F2"));
        else
        {
            GUILayout.Label("round not started");
        }

        //score
        GUILayout.Label("score: " + GameManager.Instance.Player.Score + "/" + "100");
    }
}
