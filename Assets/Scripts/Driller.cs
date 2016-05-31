using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.DrillDirection = DrillingGame.DrillingDirection.IDLE;
            GameManager.Instance.DrillingGame.Drill.rectTransform.anchoredPosition = GameManager.Instance.DrillingGame.DrillPrevPosition;

            GameManager.Instance.DrillingGame.Drill.color = new Color(1, 0, 0);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
            GameManager.Instance.DrillingGame.Bumped = true;
        }
        if (coll.gameObject.tag == "Diamond")
        {
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
            GameManager.Instance.Hud.NewFloatingText("1 point!", coll.gameObject.transform);
            Destroy(coll.gameObject);
        }
        if (coll.gameObject.tag == "GroundTile")
        {
            Destroy(coll.gameObject);
        }
        if (coll.gameObject.tag == "Cable")
        {
            GameManager.Instance.Player.CollectCable(1);

            Destroy(coll.gameObject);
        }
        if (coll.gameObject.tag == "Water")
        {
            GameManager.Instance.DrillingGame.AddWater(coll.gameObject);
            if(GameManager.Instance.DrillingGame.GetWaterCount <= 3) 
                LeanTween.scale(GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>(),
                    GameManager.Instance.DrillingGame.WaterBar.GetComponent<RectTransform>().localScale * 1.1f, 0.8f).setEase(LeanTweenType.punch);
            
            Destroy(coll.gameObject);
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            //to do in case of need
            GameManager.Instance.DrillingGame.Drill.color = new Color(1, 1, 1);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 1, 1);
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            //to do in case of need
            GameManager.Instance.DrillingGame.StuckTimer -= Time.deltaTime;
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }
}
