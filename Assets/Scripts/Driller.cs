using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    public UnityEngine.UI.Image Drill { get; private set; }

    void Start()
    {
        Drill = GetComponent<UnityEngine.UI.Image>();
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
                if(GameManager.Instance.DrillingGame.GetWaterCount <= 3) 
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
}
