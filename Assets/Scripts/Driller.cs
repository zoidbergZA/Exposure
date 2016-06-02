﻿using UnityEngine;
using System.Collections;

public class Driller : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            GameManager.Instance.DrillingGame.Drill.color = new Color(1, 0, 0);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 0, 0);
            GameManager.Instance.DrillingGame.Bumped = true;
        }
        if (coll.gameObject.tag == "Diamond")
        {
            GameManager.Instance.Player.ScorePoints(GameManager.Instance.DrillingGame.DiamondValue);
            Destroy(coll.gameObject);
        }
        if (coll.gameObject.tag == "GroundTile")
        {
            Destroy(coll.gameObject);
        }
        if (coll.gameObject.tag == "Cable")
        {
            GameManager.Instance.Player.CollectCable(2);

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
        if (coll.gameObject.tag == "Pipe")
        {
            GameManager.Instance.DrillingGame.SucceededDrill = true;
            GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.STARTSTOPTOAST;
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Rock" || coll.gameObject.tag == "Walls")
        {
            GameManager.Instance.DrillingGame.Drill.color = new Color(1, 1, 1);
            GameManager.Instance.DrillingGame.DrillLife.color = new Color(1, 1, 1);
            GameManager.Instance.DrillingGame.Bumped = false;
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
