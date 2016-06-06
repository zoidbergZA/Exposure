using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Hud : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image joystickArrow;
    [SerializeField] private float joystickArrowFadeSpeed = 2f;
    [SerializeField] private Image buildArrow;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Text timeText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject cablePanel;
    [SerializeField] private Text cableText;
    [SerializeField] private GameObject gameOverPanel;

    private int buttonSize = 55;
    private int buttonIndent = 10;
    private Vector2 pointerArrowOffset;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;

    public float WobbleValue { get; private set; }

    void Awake()
    {
        gameOverPanel.SetActive(false);
        joystickArrow.color = new Color(1, 1 , 1, 0);
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

        //joystick arrow
        if (joystickArrow.color.a > 0)
        {
            joystickArrow.color = new Color(1, 1, 1, joystickArrow.color.a - Time.deltaTime * joystickArrowFadeSpeed);
            joystickArrow.rectTransform.localPosition = GameManager.Instance.DrillingGame.Drill.rectTransform.localPosition + (Vector3)pointerArrowOffset;
        }

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

    public void PointJoystickArrow(DrillingDirection direction)
    {
        joystickArrow.color = new Color(1, 1, 1, 1);

        float rotation = 0;
//        Vector2 offset = Vector2.zero;

        switch (direction)
        {
            case DrillingDirection.UP:
                rotation = 0;
                break;
            case DrillingDirection.DOWN:
                rotation = 180;
                break;
            case DrillingDirection.LEFT:
                rotation = 90;
                break;
            case DrillingDirection.RIGHT:
                rotation = 270;
                break;
        }

//        joystickArrow.rectTransform.localPosition = offset;
        joystickArrow.transform.localEulerAngles = new Vector3(0, 0, rotation);
        joystickArrow.transform.SetAsLastSibling();
    }

    public void ShowBuildArrow(bool show)
    {
        buildArrow.enabled = show;
    }

    public void PointBuildArrow(Vector2 direction)
    {
        //        Debug.Log(Direction);

        float angle = Utils.AngleSigned(Vector3.up, (Vector3)direction, Vector3.forward);
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

        cablePanelTweenId = LeanTween.scale(cablePanel.GetComponent<RectTransform>(), cablePanel.GetComponent<RectTransform>().localScale * 1.4f, 1f)
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
