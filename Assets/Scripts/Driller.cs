using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Rock")
        {
            GameManager.Instance.DrillingGame.succeededDrill = false;
            GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        }
        else if (coll.gameObject.tag == "Diamond")
        {
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
            GameManager.Instance.Hud.NewFloatingText("1 point!", coll.gameObject.transform);
            Destroy(coll.gameObject);
        }
    }
}
