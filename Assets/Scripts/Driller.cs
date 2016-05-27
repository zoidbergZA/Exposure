using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            //GameManager.Instance.DrillingGame.SucceededDrill = false;
            //GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
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
            //to do in case of need
            GameManager.Instance.DrillingGame.StuckTimer = GameManager.Instance.DrillingGame.stuckTime;
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
