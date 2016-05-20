using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            //GameManager.Instance.DrillingGame.succeededDrill = false;
            //GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        }
        if (coll.gameObject.tag == "Diamond")
        {
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
            GameManager.Instance.Hud.NewFloatingText("1 point!", coll.gameObject.transform);
            Destroy(coll.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.CollidedRock = true;
        }
        else GameManager.Instance.DrillingGame.CollidedRock = false;
        if (coll.gameObject.tag == "Diamond")
        {
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
            GameManager.Instance.Hud.NewFloatingText("1 point!", coll.gameObject.transform);
            Destroy(coll.gameObject);
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.CollidedRock = false;
        }
        if (coll.gameObject.tag == "Diamond")
        {

        }
    }
}
