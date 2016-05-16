using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour
{
    private int buttoSize = 55;
    private int buttonIndent = 10;
    private UnityEngine.UI.Button drillButton;
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
            if (GUI.Button(new Rect(100, Screen.height - 65, buttoSize, buttoSize), "scan"))
            {
                GameManager.Instance.Scanner.Scan();
            }
        }
        else
        {
            GUI.Label(new Rect(buttonIndent, Screen.height - (buttoSize + buttonIndent), buttoSize, buttoSize), GameManager.Instance.Scanner.Cooldown.ToString("F2"));
        }
    }

    private void ShowDrillButton()
    {
        if(GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (GUI.Button(new Rect(165, Screen.height - (buttoSize + buttonIndent), buttoSize, buttoSize), "Drill!"))
            {
                GameManager.Instance.DrillingGame.SetMakeDrill(true);
            }
        }
    }

    private void ShowSteerButtons()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (GUI.Button(new Rect(100, Screen.height - (buttoSize + buttonIndent), buttoSize, buttoSize), "LEFT"))
            {
                GameManager.Instance.DrillingGame.MoveLeft();
            }

            if (GUI.Button(new Rect(165, Screen.height - (buttoSize + buttonIndent), buttoSize, buttoSize), "RIGHT"))
            {
                GameManager.Instance.DrillingGame.MoveRight();
            }
        }
    }

    private void ShowDebug()
    {
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
