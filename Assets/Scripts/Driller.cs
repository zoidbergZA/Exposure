using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Driller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Animator animatorFemale;
    [SerializeField] private Image arrowDown;
    [SerializeField] private Image arrowUp;
    [SerializeField] private Image arrowRight;
    [SerializeField] private Image arrowLeft;
    [SerializeField] private Image tapTip;
    [SerializeField] private Image pipeFeedback;
    [SerializeField] private Image diamondFeedback;
    [SerializeField] private float feedbackFadeSpeed = 1.1f;

    private int lives = 3;

    public Image Drill { get; private set; }
    public Animator Animator { get; private set; }
    public Vector2 Position
    {
        get
        {
            if (Drill) return Drill.rectTransform.anchoredPosition;
            else return Vector2.zero;
        }
        set { Drill.rectTransform.anchoredPosition = value; }
    }
    public Rigidbody2D Body { get; private set; }
    public enum Tile { ROCK, PIPE, BOMB, BOMB_AREA, DIAMOND, LIFE, ELECTRICITY, GROUND_TILE, PIPE_PART }
    public enum DrillerGender { MALE, FEMALE }
    public int Lives { get { return lives; } }
    public DrillGameHud Hud { get; private set; }
    public bool Collided { get; private set; }
    public Image ArrowDown { get { return arrowDown; } }
    public Image ArrowUp { get { return arrowUp; } }
    public Image ArrowRight { get { return arrowRight; } }
    public Image ArrowLeft { get { return arrowLeft; } }
    public Image TapTip { get { return tapTip; } }
    public DrillerGender Gender { get; set; }

    void Awake()
    {
        //cheat random Gender assignment
        int randomGender = Random.Range(0, 2);
        if (randomGender == 0) Gender = DrillerGender.MALE;
        else Gender = DrillerGender.FEMALE;

        Body = GetComponent<Rigidbody2D>();
        Hud = FindObjectOfType<DrillGameHud>();
    }

    void Start()
    {
        if (Gender == DrillerGender.MALE)
        {
            Animator = animator;
            animator.gameObject.GetComponent<Image>().enabled = true;
            animatorFemale.gameObject.SetActive(false); //switch off female if male
        }
        else
        {
            Animator = animatorFemale;
            animatorFemale.gameObject.GetComponent<Image>().enabled = true;
            animator.gameObject.SetActive(false); //switch off male if female
        }
        Drill = FindObjectOfType<Driller>().GetComponent<Image>();
    }

    void Update()
    {
        Body.inertia = 0;
        Body.freezeRotation = true;

        float displacement = Mathf.Sin(Time.time);
        arrowDown.rectTransform.localPosition = new Vector2(arrowDown.rectTransform.localPosition.x, arrowDown.rectTransform.localPosition.y + displacement/2);
        arrowRight.rectTransform.localPosition = new Vector2(arrowRight.rectTransform.localPosition.x + displacement, arrowRight.rectTransform.localPosition.y);

        updateFeedbackImages();
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        switch(coll.gameObject.tag)
        {
            case "Rock": case "Walls":
                handleCollision(Tile.ROCK);
                break;
            case "Diamond":
                handleCollision(Tile.DIAMOND, coll.gameObject);
                break;
            case "GroundTile":
                handleCollision(Tile.GROUND_TILE, coll.gameObject);
                break;
            case "Cable":
                handleCollision(Tile.ELECTRICITY, coll.gameObject);
                break;
            case "Water":
                handleCollision(Tile.PIPE_PART, coll.gameObject);
                break;
            case "Pipe":
                handleCollision(Tile.PIPE);
                break;
            case "Mine":
                handleCollision(Tile.BOMB);
                break;
            case "MineArea":
                handleCollision(Tile.BOMB_AREA);
                break;
            case "DrillLife":
                handleCollision(Tile.LIFE, coll.gameObject);
                break;
        }
    }

    private void resetAnimation()
    {
        Animator.SetBool("isSlidingLeft", false);
        Animator.SetBool("isDrillingDown", false);
        Animator.SetBool("isDrillingUp", false);
        Animator.SetBool("isDrillingRight", false);
        Animator.SetBool("isDrillingLeft", false);
        Animator.SetBool("shouldJump", false);
        Animator.SetBool("goToSliding", false);
    }

    public void Reset(Vector2 startPosition)
    {
        resetAnimation();
        if (!GameManager.Instance.DrillingGame.IsRestarting) lives = 3;
        Drill.rectTransform.anchoredPosition = startPosition;
        Drill.gameObject.SetActive(false);
        Collided = false;
        pipeFeedback.color = new Color(1, 1, 1, 0);
        diamondFeedback.color = new Color(1, 1, 1, 0);
        ActivateImage(arrowRight, false);
    }

    public void SwitchAnimation(string param, bool turned)
    {
        Animator.SetBool(param, turned);
    }

    public void handleCollision(Tile collider, GameObject GO = null)
    {
        switch(collider)
        {
            case Tile.BOMB:
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                if (lives == 3) updateDrillerLife(-3);
                else lives = 0;
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.EXPLODED_BOMB;
                Collided = true;
                break;
            case Tile.BOMB_AREA:
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_DRILL; //also TRIGGERED_BOMB available
                Collided = true;
                break;
            case Tile.DIAMOND:
                diamondFeedback.color = new Color(1, 1, 1, 1);
                GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
                Destroy(GO);
                break;
            case Tile.ELECTRICITY:
                GameManager.Instance.Player.CollectCable(2);
                Destroy(GO);
                break;
            case Tile.LIFE:
                if (lives < 3)
                {
                    updateDrillerLife(1);
                    LeanTween.scale(Hud.DrillLife.GetComponent<RectTransform>(), Hud.DrillLife.GetComponent<RectTransform>().localScale * 1.1f, 0.8f)
                        .setEase(LeanTweenType.punch);
                }
                Destroy(GO);
                break;
            case Tile.PIPE:
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.TIME_OUT;
                Collided = true;
                break;
            case Tile.ROCK:
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_DRILL;
                Collided = true;
                break;
            case Tile.GROUND_TILE:
                Destroy(GO);
                break;
            case Tile.PIPE_PART:
                pipeFeedback.color = new Color(1, 1, 1, 1);
                GameManager.Instance.DrillingGame.Map.AddPipePart(GO.GetComponent<DrillingGameTile>());
                if (GameManager.Instance.DrillingGame.Map.GetPipePartsCount <= 3)
                    LeanTween.scale(Hud.WaterBar.GetComponent<RectTransform>(),
                        Hud.WaterBar.GetComponent<RectTransform>().localScale * 1.1f, 0.8f).setEase(LeanTweenType.punch);
                Destroy(GO);
                break;
        }
    }

    private void updateDrillerLife(int amount)
    {
        lives += amount;
    }

    public void ActivateImage(Image arrow, bool activate)
    {
        arrow.enabled = activate;
    }

    private void updateFeedbackImages()
    {
        if (pipeFeedback.color.a > 0 && GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            pipeFeedback.gameObject.SetActive(true);
            pipeFeedback.color = new Color(1, 1, 1, pipeFeedback.color.a - Time.deltaTime * feedbackFadeSpeed);
        }
        if (diamondFeedback.color.a > 0 && GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            diamondFeedback.gameObject.SetActive(true);
            diamondFeedback.color = new Color(1, 1, 1, diamondFeedback.color.a - Time.deltaTime * feedbackFadeSpeed);
        }
    }
}
