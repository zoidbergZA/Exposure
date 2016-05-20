using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Texture2D leftButton;
    [SerializeField] private Texture2D rightButton;
    [SerializeField] private Texture2D drillButton;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject cablePanel;
    [SerializeField] private Text cableText;
    [SerializeField] private GameObject gameOverPanel;
    private int buttoSize = 55;
    private int buttonIndent = 10;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;

    public float WobbleValue { get; private set; }

    void Awake()
    {
        gameOverPanel.SetActive(false);
        wobblerTweenId = LeanTween.value(gameObject, updateWobbleCallback, 0f, 1f, 0.6f).setLoopPingPong().setEase(LeanTweenType.easeInOutSine).id;
    }

    void Update()
    {
        scoreText.text = GameManager.Instance.Player.Score.ToString();
        cableText.text = GameManager.Instance.Player.Cable + "x";
    }

    void OnGUI()
    {
        ShowScanButton();
        ShowDebug();
        ShowDrillButton();
        ShowSteerButtons();
    }

    public void GoToGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShakeScorePanel()
    {
        if (LeanTween.isTweening(scorePanelTweenId))
        {
            LeanTween.cancel(scorePanelTweenId);
            scorePanel.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        scorePanelTweenId = LeanTween.scale(scorePanel.GetComponent<RectTransform>(), scorePanel.GetComponent<RectTransform>().localScale * 1.4f, 1f)
            .setEase(LeanTweenType.punch).id;
    }

    public void ShakeCablePanel()
    {
        if (LeanTween.isTweening(cablePanelTweenId))
        {
            LeanTween.cancel(cablePanelTweenId);
            cablePanel.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        cablePanelTweenId = LeanTween.scale(cablePanel.GetComponent<RectTransform>(), cablePanel.GetComponent<RectTransform>().localScale*1.4f,1f)
            .setEase(LeanTweenType.punch).id;
    }

    public void NewFloatingText(string text, Transform target)
    {
        FloatingText ft = (FloatingText)Instantiate(floatingTextPrefab, Vector3.zero, Quaternion.identity);
        ft.RectTransform.SetParent(hudCanvas.GetComponent<RectTransform>());

        ft.Init(text, target);

    }

    public void ShowWorldSpaceButton(Texture2D icon, Vector3 position, Action callback)
    {
        float wobbleValue = WobbleValue * 10f;

        if (GUI.Button(new Rect(position.x - 20 - wobbleValue / 2, Screen.height - position.y - 20 - wobbleValue / 2, 40 + wobbleValue, 40 + wobbleValue), icon, ""))
        {
            callback();
        }
    }

    private void ShowScanButton()
    {
        if (GameManager.Instance.Scanner.IsReady && GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
        {
            if (GUI.Button(new Rect(Screen.width - 65, Screen.height - buttoSize - 10, buttoSize, buttoSize), "scan"))
            {
                GameManager.Instance.Scanner.Scan();
            }
        }
        else
        {
            GUI.Label(new Rect(Screen.width - 65, 10, buttoSize, buttoSize), GameManager.Instance.Scanner.Cooldown.ToString("F2"));
        }
    }

    private void ShowDrillButton()
    {
        if(GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (GUI.Button(new Rect(((Screen.width / 3) / 2)-50, (Screen.height / 2) + 290, 100, 80), drillButton, ""))
            {
                GameManager.Instance.DrillingGame.SetMakeDrill(true);
            }
        }
    }

    private void ShowSteerButtons()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (GUI.Button(new Rect(((Screen.width/3)/2)-180, (Screen.height/2) + 290, 100, 70), leftButton, ""))
            {
                GameManager.Instance.DrillingGame.MoveLeft();
            }

            if (GUI.Button(new Rect(((Screen.width/3)/2)+80, (Screen.height/2) + 290, 100, 70), rightButton, ""))
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

    void updateWobbleCallback(float val, float ratio)
    {
        WobbleValue = val;
    }
}
