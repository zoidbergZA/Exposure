using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Hud : MonoBehaviour
{
    public string[] PositiveMessages;
    public string[] NegativeMessages;

    [SerializeField] private Sprite fullStar;
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Image starImagePrefab;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private RectTransform scoreStarPanel;
    [SerializeField] private Image tipBubble;
    [SerializeField] private Sprite[] tipSprites;
    [SerializeField] private GameObject scannerTip;
    [SerializeField] private Image buildArrow;
    [SerializeField] private GameObject cityPanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject timePanel;
    [SerializeField] private Text timeText;
//    [SerializeField] private Text gameOverText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text endScoreText;
    [SerializeField] private Text endPlayerText;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject cablePanel;
    [SerializeField] private Text cableText;
    [SerializeField] private GameObject gameOverPanel;

    private int buttonSize = 85;
    private int buttonIndent = 10;

    private float tipTimeRemaing;
    private Transform tipTargeTransform;

    private int wobblerTweenId;
    private int scorePanelTweenId;
    private int cablePanelTweenId;

    public float WobbleValue { get; private set; }
    public CitiesBar CitiesBar { get; private set; }
    public bool ShowingScannerTip { get; private set; }

    void Awake()
    {
        CitiesBar = GetComponentInChildren<CitiesBar>();
        gameOverPanel.SetActive(false);
        tipBubble.enabled = false;
        wobblerTweenId =
            LeanTween.value(gameObject, updateWobbleCallback, 0f, 1f, 0.6f)
                .setLoopPingPong()
                .setEase(LeanTweenType.easeInOutSine)
                .id;
    }

    void Start()
    {
        scorePanel.SetActive(false);
        timePanel.SetActive(false);
        cityPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    void Update()
    {
        //test

        if (Input.GetKeyDown(KeyCode.T))
            GoToGameOver(42);

        //test

        //timeleft
        int minutes = Mathf.FloorToInt(GameManager.Instance.TimeLeft/60F);
        int seconds = Mathf.FloorToInt(GameManager.Instance.TimeLeft - minutes*60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

        timeText.text = niceTime;
        scoreText.text = GameManager.Instance.Player.Score.ToString();
        cableText.text = GameManager.Instance.Player.Cable.ToString();

        //scanner tip
        if (scannerTip.activeSelf)
        {
            scannerTip.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(GameManager.Instance.ScannerGadget.transform.position);
        }

        //tip bubble
        if (tipTimeRemaing > 0)
        {
            tipBubble.rectTransform.position = Camera.main.WorldToScreenPoint(tipTargeTransform.position);
            tipTimeRemaing -= Time.deltaTime;

            if (tipTimeRemaing <= 0)
            {
                tipBubble.enabled = false;
            }
        }
    }

    void OnGUI()
    {
        if (GameManager.Instance.showDebug)
            ShowDebug();
    }

    public void ShowTipBubble(Transform refTransform, float duration = 3f)
    {
        Sprite tipSprite = null;
        
        
        tipSprite = tipSprites[UnityEngine.Random.Range(0, tipSprites.Length)];
        

        tipBubble.sprite = tipSprite;
        tipBubble.rectTransform.position = Camera.main.WorldToScreenPoint(refTransform.position);
        tipBubble.enabled = true;

        tipTimeRemaing = duration;
        tipTargeTransform = refTransform;
    }

    public Rect CenteredRect(Rect rect)
    {
        Rect output = new Rect(
            rect.x - rect.width/2f,
            Screen.height - rect.y - rect.height/2f,
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
        //        Debug.Log(Direction);

        float angle = Utils.AngleSigned(Vector3.up, (Vector3) direction, Vector3.forward);
        buildArrow.rectTransform.eulerAngles = new Vector3(0, 0, angle);

        //        Debug.Log(angle);
    }

    public void OnStartRoundClicked()
    {
        ShowStartPanel(false);

        scorePanel.SetActive(true);
        timePanel.SetActive(true);
        cityPanel.SetActive(true);

        GameManager.Instance.Director.SetSunlightBrightness(1f);
        GameManager.Instance.StartRound();
    }

    public void GoToGameOver(int score)
    {
        gameOverPanel.SetActive(true);

        Image[] scoreStarImages = new Image[GameManager.Instance.Cities.Length];

        for (int i = 0; i < GameManager.Instance.Cities.Length; i++)
        {
            scoreStarImages[i] = Instantiate(starImagePrefab);
            scoreStarImages[i].rectTransform.SetParent(scoreStarPanel);
            scoreStarImages[i].rectTransform.localScale = Vector3.one;

            if (!GameManager.Instance.Cities[i].IsDirty)
            {
                scoreStarImages[i].sprite = fullStar;
            }
        }

        for (int i = 0; i < scoreStarImages.Length; i++)
        {
            if (scoreStarImages[i].sprite == fullStar)
                scoreStarImages[i].rectTransform.SetAsFirstSibling();
        }

        Destroy(starImagePrefab.gameObject);
        
        //set score and player name text
        endPlayerText.text = "Goed Gedaan " + GameManager.Instance.Player.PlayerName + "!!!";
        endScoreText.text = score.ToString();
    }

    public void ShakeScorePanel()
    {
        if (LeanTween.isTweening(scorePanelTweenId))
        {
            LeanTween.cancel(scorePanelTweenId);
            scorePanel.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        scorePanelTweenId =
            LeanTween.scale(scorePanel.GetComponent<RectTransform>(),
                scorePanel.GetComponent<RectTransform>().localScale*1.4f, 1f)
                .setEase(LeanTweenType.punch).id;
    }

    public void ShakeCablePanel()
    {
        if (LeanTween.isTweening(cablePanelTweenId))
        {
            LeanTween.cancel(cablePanelTweenId);
            cablePanel.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        cablePanelTweenId =
            LeanTween.scale(cablePanel.GetComponent<RectTransform>(),
                cablePanel.GetComponent<RectTransform>().localScale*1.4f, 1f)
                .setEase(LeanTweenType.punch).id;
    }

    public void NewFloatingText(string text, Transform target, bool positive = true)
    {
        FloatingText ft = (FloatingText) Instantiate(floatingTextPrefab, Vector3.zero, Quaternion.identity);
        ft.RectTransform.SetParent(hudCanvas.GetComponent<RectTransform>());

        ft.Init(text, target, positive);

    }

    public void ShowScannerTip(bool show)
    {
        if (ShowingScannerTip == show)
            return;

        ShowingScannerTip = show;
        
        scannerTip.SetActive(show);
    }

public void ShowWorldSpaceButton(Texture2D icon, Vector3 position, Action callback)
    {
        float wobbleValue = WobbleValue * 13f;

        if (GUI.Button(CenteredRect(new Rect(position.x, position.y, buttonSize + wobbleValue, buttonSize + wobbleValue)), icon, ""))
        {
            callback();
        }
    }

    private void ShowStartPanel(bool show)
    {
        startPanel.SetActive(show);
    }

    private void ShowDebug()
    {
//        GUILayout.BeginArea(new Rect(10, 60, 400, 300));
//
//        GUILayout.Label("time left: " + GameManager.Instance.TimeLeft.ToString("F2"));
//
//        GUILayout.Label("scanner max radius: " + GameManager.Instance.Scanner.maxRadius);
//        GameManager.Instance.Scanner.maxRadius = GUILayout.HorizontalSlider(GameManager.Instance.Scanner.maxRadius, 2f, 100f);
//
//        GUILayout.Label("scanner max distance: " + GameManager.Instance.Scanner.limiter);
//        GameManager.Instance.Scanner.limiter = GUILayout.HorizontalSlider(GameManager.Instance.Scanner.limiter, 50f, 400f);
//
//        GUILayout.EndArea();
    }

    void updateWobbleCallback(float val, float ratio)
    {
        WobbleValue = val;
    }
}
