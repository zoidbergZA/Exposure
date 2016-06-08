using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public UnityEngine.UI.Image Drill { get; private set; }
    public Animator Animator { get { return animator; } }
    public const int ANCHORED_X_OFFSET = 112;
    public const int ANCHORED_Y_OFFSET = 690;
    public Vector2 Position
    { 
        get
        { 
            return new Vector2(Drill.rectTransform.anchoredPosition.x - ANCHORED_X_OFFSET, Drill.rectTransform.anchoredPosition.y - ANCHORED_Y_OFFSET); 
        }
        set
        { 
            Drill.rectTransform.anchoredPosition = value;
            Drill.rectTransform.anchoredPosition = 
                new Vector2(Drill.rectTransform.anchoredPosition.x + ANCHORED_X_OFFSET, Drill.rectTransform.anchoredPosition.y + ANCHORED_Y_OFFSET);
        }
    }
    public Rigidbody2D Body { get; private set; }

    void Awake()
    {
        Drill = GetComponent<UnityEngine.UI.Image>();
        Body = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Body.inertia = 0;
        Body.freezeRotation = true;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        switch(coll.gameObject.tag)
        {
            case "Rock": case "Walls":
                handleRockCollision(true);
                break;
            case "Diamond":
                GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
                Destroy(coll.gameObject);
                break;
            case "GroundTile":
                Destroy(coll.gameObject);
                break;
            case "Cable":
                GameManager.Instance.Player.CollectCable(2);
                Destroy(coll.gameObject);
                break;
            case "Water":
                GameManager.Instance.DrillingGame.Map.AddWater(coll.gameObject.GetComponent<DrillingGameTile>());
                if(GameManager.Instance.DrillingGame.Map.GetWaterCount <= 3) 
                    LeanTween.scale(GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>(),
                        GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>().localScale * 1.1f, 0.8f).setEase(LeanTweenType.punch);
                Destroy(coll.gameObject);
                break;
            case "Pipe":
                handleRockCollision(true);
                //handlePipeCollision();
                break;
            case "Mine":
                //todo the same as pipe but with different animation
                handlePipeCollision();
                break;
            case "MineArea":
                //todo countdown
                handlePipeCollision();
                break;
            case "DrillLife":
                //todo - complete function
                handleDrillLifeCollision();
                Destroy(coll.gameObject);
                break;
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            handleRockCollision(false);
        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            GameManager.Instance.DrillingGame.StuckTimer -= Time.deltaTime;
        }
    }

    public void resetAnimation()
    {
        animator.SetBool("isSlidingLeft", false);
        animator.SetBool("isDrillingDown", false);
        animator.SetBool("isDrillingUp", false);
        animator.SetBool("isDrillingRight", false);
        animator.SetBool("isDrillingLeft", false);
        animator.SetBool("shouldJump", false);
    }

    public void SwitchAnimation(string param, bool turned)
    {
        animator.SetBool(param, turned);
    }

    public void handleRockCollision(bool entered)
    {
        if (entered)
        {
            Drill.color = new Color(1, 0, 0);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
        }
        else
        {
            Drill.color = new Color(1, 1, 1);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 1, 1);
        }
        GameManager.Instance.DrillingGame.Bumped = true;
    }

    public void handlePipeCollision()
    {
        GameManager.Instance.DrillingGame.SucceededDrill = false;
        GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_PIPE;
        Drill.color = new Color(1, 0, 0);
        GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
    }

    public void handleMineCollision()
    {
        GameManager.Instance.DrillingGame.SucceededDrill = false;
        GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        GameManager.Instance.DrillingGame.ToastType = global::ToastType.EXPLODED_BOMB;
        Drill.color = new Color(1, 0, 0);
        GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
    }

    public void handleMineAreaCollision()
    {
        GameManager.Instance.DrillingGame.SucceededDrill = false;
        GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        GameManager.Instance.DrillingGame.ToastType = global::ToastType.TRIGGERED_BOMB;
        Drill.color = new Color(1, 0, 0);
        GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
    }
    public void handleDrillLifeCollision()
    {
        GameManager.Instance.Player.ScorePoints(5);
    }
}
