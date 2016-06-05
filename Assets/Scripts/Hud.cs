using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Hud : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image buildArrow;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Text timeText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject cablePanel;
    [SerializeField] private Text cableText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private UnityEngine.UI.Button upButton;
    [SerializeField] private UnityEngine.UI.Button leftButton;
    [SerializeField] private UnityEngine.UI.Button rightButton;
    [SerializeField] private UnityEngine.UI.Button downButton;
    [SerializeField] private int buttonSize = 55;
    private int buttonIndent = 10;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;
    private float joystickX, joystickY = 0;

    public float WobbleValue { get; private set; }
    public UnityEngine.UI.Button DrillButton { get { return upButton; } }
    public UnityEngine.UI.Button LeftButton { get { return leftButton; } }
    public UnityEngine.UI.Button RightButton { get { return rightButton; } }
    public UnityEngine.UI.Button DownButton { get { return downButton; } }
    public float JoystickX { get { return joystickX; } set { joystickX = value; } }
    public float JoystickY { get { return joystickY; } set { joystickY = value; } }

    void Awake()
    {
        gameOverPanel.SetActive(false);
        wobblerTweenId = LeanTween.value(gameObject, updateWobbleCallback, 0f, 1f, 0.6f).setLoopPingPong().setEase(LeanTweenType.easeInOutSine).id;
    }

    void Update()
    {
        //timeleft
        int minutes = Mathf.FloorToInt(GameManager.Instance.TimeLeft / 60F);
        int seconds = Mathf.FloorToInt(GameManager.Instance.TimeLeft - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        timeText.text = niceTime;
        scoreText.text = GameManager.Instance.Player.Score.ToString();
        cableText.text = GameManager.Instance.Player.Cable.ToString();
        
        //updateMiniGameButtons();
        updateJoystick();

//        //arrow test
//        City closestCity = GameManager.Instance.GridBuilder.FindClosestCity(Vector3.zero);
//        Vector3 cityScreenPos = Camera.main.WorldToScreenPoint(closestCity.transform.position);
//
//        Vector2 dir = (Vector2)cityScreenPos - new Vector2(Screen.width/2, Screen.height/2);
//        PointBuildArrow(dir);
    }

    void OnGUI()
    {
        if (GameManager.Instance.showDebug)
            ShowDebug();
    }

    public Rect CenteredRect(Rect rect)
    {
        Rect output = new Rect(
                rect.x - rect.width / 2f,
                Screen.height - rect.y - rect.height / 2f,
                rect.width,
                rect.height
            );

        return output;
    }

    public void ShowBuildArrow(bool show)
    {
        buildArrow.enabled = show;
    }

    public void PointBuildArrow(Vector2 direction)
    {
//        Debug.Log(direction);

        float angle = Utils.AngleSigned(Vector3.up, (Vector3) direction, Vector3.forward);
        buildArrow.rectTransform.eulerAngles = new Vector3(0, 0, angle);

//        Debug.Log(angle);
    }

    public void GoToGameOver(int score)
    {
        gameOverPanel.SetActive(true);

        gameOverText.text = "score: " + score;
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
        float wobbleValue = WobbleValue * 13f;

        if (GUI.Button(CenteredRect(new Rect(position.x, position.y, buttonSize + wobbleValue, buttonSize + wobbleValue)), icon, ""))
        {
            callback();
        }
    }

    private void updateJoystick()
    {
        if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            if (joystickX >= 0.907f) //normally should be 0.707 (sin 45 deg)
            {
                //right
                if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.RIGHT)
                    GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
                if (GameManager.Instance.DrillingGame.PrevDrillDirection != DrillingGame.DrillingDirection.LEFT
                    && GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.RIGHT)
                {
                    GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.RIGHT;
                    GameManager.Instance.DrillingGame.JustTurned = true;
                }
            }
            else if (joystickX <= -0.907) //normally should be -0.707 (sin 45 deg)
            {
                //left
                if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.LEFT)
                    GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
                if (GameManager.Instance.DrillingGame.PrevDrillDirection != DrillingGame.DrillingDirection.RIGHT
                    && GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.LEFT)
                {
                    GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.LEFT;
                    GameManager.Instance.DrillingGame.JustTurned = true;
                }
            }
            else if (joystickY >= 0.907f) //normally should be 0.707 (cos 45 deg)
            {
                //up
                if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.UP)
                    GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
                if (GameManager.Instance.DrillingGame.PrevDrillDirection != DrillingGame.DrillingDirection.DOWN
                    && GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.UP)
                {
                    GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.UP;
                    GameManager.Instance.DrillingGame.JustTurned = true;
                }
            }
            else if (joystickY <= -0.907f) //normally should be -0.707 (cos 45 deg)
            {
                //down
                if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.DOWN)
                    GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
                if (GameManager.Instance.DrillingGame.PrevDrillDirection != DrillingGame.DrillingDirection.UP
                    && GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.DOWN)
                {
                    GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.DOWN;
                    GameManager.Instance.DrillingGame.JustTurned = true;
                }
            }
        }
        else if (GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.SLIDING)
        {
            if (joystickY < -0.707f)
            {
                GameManager.Instance.DrillingGame.MakeDrill(true);
                GameManager.Instance.DrillingGame.Animator.SetBool("shouldJump", true);
            }
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
        if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.LEFT)
        {
            GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        }
        GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.LEFT;
    }

    public void HandleRightButton()
    {
        if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.RIGHT)
        {
            GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        }
        GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.RIGHT;
    }

    public void HandleUpButton()
    {
        if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.UP)
        {
            GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
        }
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
            if (GameManager.Instance.DrillingGame.DrillDirection != DrillingGame.DrillingDirection.DOWN)
            {
                GameManager.Instance.DrillingGame.PrevDrillDirection = GameManager.Instance.DrillingGame.DrillDirection;
            }
            GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.DOWN;
        }
    }

    private void ShowDebug()
    {
        GUILayout.BeginArea(new Rect(10, 60, 400, 300));

        GUILayout.Label("time left: " + GameManager.Instance.TimeLeft.ToString("F2"));

        GUILayout.Label("scanner max radius: " + GameManager.Instance.Scanner.maxRadius);
        GameManager.Instance.Scanner.maxRadius = GUILayout.HorizontalSlider(GameManager.Instance.Scanner.maxRadius, 2f, 100f);

        GUILayout.Label("scanner max distance: " + GameManager.Instance.Scanner.limiter);
        GameManager.Instance.Scanner.limiter = GUILayout.HorizontalSlider(GameManager.Instance.Scanner.limiter, 50f, 400f);

        GUILayout.EndArea();
    }

    void updateWobbleCallback(float val, float ratio)
    {
        WobbleValue = val;
    }
}
