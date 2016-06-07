using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public UnityEngine.UI.Image Drill { get; private set; }
    public Animator Animator { get { return animator; } }
    public const int ANCHORED_OFFSET = 112;
    public Vector2 Position { 
        get { return Drill.rectTransform.anchoredPosition; }
        set { Drill.rectTransform.anchoredPosition = value; }
    }

    void Awake()
    {
        Drill = GetComponent<UnityEngine.UI.Image>();
    }

    void Start()
    {
        
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        switch(coll.gameObject.tag)
        {
            case "Rock": case "Walls":
                GameManager.Instance.DrillingGame.handleRockCollision(true);
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
                GameManager.Instance.DrillingGame.handlePipeCollision();
                break;
            case "Mine":
                //todo the same as pipe but with different animation
                GameManager.Instance.DrillingGame.handlePipeCollision();
                break;
            case "MineArea":
                //todo countdown
                GameManager.Instance.DrillingGame.handlePipeCollision();
                break;
            case "DrillLife":
                //todo
                break;
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            GameManager.Instance.DrillingGame.handleRockCollision(false);
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            GameManager.Instance.DrillingGame.StuckTimer -= Time.deltaTime;
        }
        if (coll.gameObject.tag == "Diamond")
        {

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
}
