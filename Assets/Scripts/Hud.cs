﻿using System;
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
    [SerializeField] private UnityEngine.UI.Button upButton;
    [SerializeField] private UnityEngine.UI.Button leftButton;
    [SerializeField] private UnityEngine.UI.Button rightButton;
    [SerializeField] private UnityEngine.UI.Button downButton;
    private int buttonSize = 55;
    private int buttonIndent = 10;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;

    public float WobbleValue { get; private set; }
    public UnityEngine.UI.Button DrillButton { get { return upButton; } }
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
        ShowDebug();
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
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            leftButton.interactable = false;
            rightButton.interactable = false;
            downButton.interactable = true;
            upButton.interactable = false;
        }
        else
        {
            switch(GameManager.Instance.DrillingGame.DrillDirection)
            {
                case DrillingGame.DrillingDirection.DOWN:
                    leftButton.interactable = true;
                    rightButton.interactable = true;
                    downButton.interactable = true;
                    upButton.interactable = false;
                    break;
                case DrillingGame.DrillingDirection.UP:
                    leftButton.interactable = true;
                    rightButton.interactable = true;
                    downButton.interactable = false;
                    upButton.interactable = true;
                    break;
                case DrillingGame.DrillingDirection.LEFT:
                    leftButton.interactable = true;
                    rightButton.interactable = false;
                    downButton.interactable = true;
                    upButton.interactable = true;
                    break;
                case DrillingGame.DrillingDirection.RIGHT:
                    leftButton.interactable = false;
                    rightButton.interactable = true;
                    downButton.interactable = true;
                    upButton.interactable = true;
                    break;
            }
        }
    }
    
    public void HandleLeftButton()
    {
        GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.LEFT;
    }

    public void HandleRightButton()
    {
        GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.RIGHT;
    }

    public void HandleUpButton()
    {
        GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.UP;
    }

    public void HandleDownButton()
    {
        if(GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            GameManager.Instance.DrillingGame.MakeDrill(true);
            GameManager.Instance.DrillingGame.Animator.SetBool("shouldJump", true);
        }
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
            GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.DOWN;
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
