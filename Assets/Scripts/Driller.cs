using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            if (GameManager.Instance.DrillingGame.MovingLeft)
            {
                GameManager.Instance.DrillingGame.MovingLeft = false;
                GameManager.Instance.DrillingGame.Bumped = true;
            }
            if (GameManager.Instance.DrillingGame.MovingRight)
            {
                GameManager.Instance.DrillingGame.MovingRight = false;
                GameManager.Instance.DrillingGame.Bumped = true;
            }
            if (GameManager.Instance.DrillingGame.Bumped == false) GameManager.Instance.DrillingGame.Bumped = true;
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
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.StuckTimer = GameManager.Instance.DrillingGame.stuckTime;
            if (GameManager.Instance.DrillingGame.Bumped == true) GameManager.Instance.DrillingGame.Bumped = false;
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.StuckTimer -= Time.deltaTime;
            if (GameManager.Instance.DrillingGame.MovingLeft) GameManager.Instance.DrillingGame.Bumped = true;
            if (GameManager.Instance.DrillingGame.MovingRight) GameManager.Instance.DrillingGame.Bumped = true;
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }
}
