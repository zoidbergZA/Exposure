using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject cablePanel;
    [SerializeField] private Text cableText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private UnityEngine.UI.Button drillButton;
    [SerializeField] private UnityEngine.UI.Button leftButton;
    [SerializeField] private UnityEngine.UI.Button rightButton;
    [SerializeField] private UnityEngine.UI.Button downButton;
    private int buttonSize = 55;
    private int buttonIndent = 10;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;

    public float WobbleValue { get; private set; }
    public UnityEngine.UI.Button DrillButton { get { return drillButton; } }
    public UnityEngine.UI.Button LeftButton { get { return leftButton; } }
    public UnityEngine.UI.Button RightButton { get { return rightButton; } }
    public UnityEngine.UI.Button DownButton { get { return downButton; } }

    void Awake()
    {
        gameOverPanel.SetActive(false);
        wobblerTweenId = LeanTween.value(gameObject, updateWobbleCallback, 0f, 1f, 0.6f).setLoopPingPong().setEase(LeanTweenType.easeInOutSine).id;
    }

    void Update()
    {
        scoreText.text = GameManager.Instance.Player.Score.ToString();
        cableText.text = GameManager.Instance.Player.Cable + "x";
        updateMiniGameButtons();
    }

    void OnGUI()
    {
//        ShowScanButton();
        ShowDebug();
        //ShowDrillButton();
        //ShowSteerButtons();
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

    private void updateMiniGameButtons()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING) drillButton.gameObject.SetActive(true);
        else drillButton.gameObject.SetActive(false);

        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            downButton.gameObject.SetActive(true);
        }
        else
        {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);
        }
    }

    public void HandleDrillButton()
    {
        GameManager.Instance.DrillingGame.SetMakeDrill(true);
        GameManager.Instance.DrillingGame.Animator.SetBool("shouldJump", true);
        Debug.Log("drill button clicked!");
    }

    public void HandleLeftButton()
    {
        GameManager.Instance.DrillingGame.MoveLeft();
        Debug.Log("left button clicked!");
    }

    public void HandleRightButton()
    {
        GameManager.Instance.DrillingGame.MoveRight();
        Debug.Log("right button clicked!");
    }

    public void HandleDownButton()
    {
        if (GameManager.Instance.DrillingGame.MovingRight)
        {
            GameManager.Instance.DrillingGame.MovingRight = false;
            GameManager.Instance.DrillingGame.WasMovingRight = true;
        }
        else GameManager.Instance.DrillingGame.WasMovingRight = false;
        if (GameManager.Instance.DrillingGame.MovingLeft)
        {
            GameManager.Instance.DrillingGame.MovingLeft = false;
            GameManager.Instance.DrillingGame.WasMovingLeft = true;
        }
        else GameManager.Instance.DrillingGame.WasMovingLeft = false;
        Debug.Log("down button clicked!");
    }

//    private void ShowScanButton()
//    {
//        if (GameManager.Instance.Scanner.IsReady && GameManager.Instance.Player.PlayerState == Player.PlayerStates.Normal)
//        {
//            if (GUI.Button(new Rect(Screen.width - 65, Screen.height - buttonSize - 10, buttonSize, buttonSize), "scan"))
//            {
//                GameManager.Instance.Scanner.Scan();
//            }
//        }
//        else
//        {
//            GUI.Label(new Rect(Screen.width - 65, Screen.height - buttonSize - 10, buttonSize, buttonSize), GameManager.Instance.Scanner.Cooldown.ToString("F2"));
//        }
//    }

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
