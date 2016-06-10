using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public UnityEngine.UI.Image Drill { get; private set; }
    public Animator Animator { get { return animator; } }
    public const int ANCHORED_X_OFFSET = 112;
    public const int ANCHORED_Y_OFFSET = 690;
    public Vector2 Position { get { return Drill.rectTransform.anchoredPosition; } set { Drill.rectTransform.anchoredPosition = value; } }
    public Rigidbody2D Body { get; private set; }
    public enum Tile { ROCK, PIPE, BOMB, BOMB_AREA, DIAMOND, LIFE, ELECTRICITY, GROUND_TILE, WATER }

    void Awake()
    {
        Drill = GetComponent<UnityEngine.UI.Image>();
        Body = GetComponent<Rigidbody2D>();
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
                handleCollision(Tile.WATER, coll.gameObject);
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
                handleCollision(Tile.LIFE);
                break;
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

    public void handleCollision(Tile collider, GameObject GO = null)
    {
        switch(collider)
        {
            case Tile.BOMB:
                //todo all lives--
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.EXPLODED_BOMB;
                GameManager.Instance.DrillingGame.SucceededDrill = false;
                GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
                break;
            case Tile.BOMB_AREA:
                //todo all lives--
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.TRIGGERED_BOMB;
                GameManager.Instance.DrillingGame.SucceededDrill = false;
                GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
                break;
            case Tile.DIAMOND:
                GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
                Destroy(GO);
                break;
            case Tile.ELECTRICITY:
                GameManager.Instance.Player.CollectCable(2);
                Destroy(GO);
                break;
            case Tile.LIFE:
                //todo life++
                Destroy(GO);
                break;
            case Tile.PIPE:
                //todo life--
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_PIPE;
                GameManager.Instance.DrillingGame.SucceededDrill = false;
                GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
                break;
            case Tile.ROCK:
                //todo life--
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_DRILL;
                GameManager.Instance.DrillingGame.SucceededDrill = false;
                GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
                break;
            case Tile.GROUND_TILE:
                Destroy(GO);
                break;
            case Tile.WATER:
                GameManager.Instance.DrillingGame.Map.AddWater(GO.GetComponent<DrillingGameTile>());
                if (GameManager.Instance.DrillingGame.Map.GetWaterCount <= 3)
                {
                    LeanTween.scale(GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>(),
                        GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>().localScale * 1.1f, 0.8f).setEase(LeanTweenType.punch);
                }
                Destroy(GO);
                break;
        }
    }
}
