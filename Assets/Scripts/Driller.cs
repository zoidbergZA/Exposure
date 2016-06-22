﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Driller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Image arrowDown;
    [SerializeField] private Image arrowUp;
    [SerializeField] private Image arrowRight;
    [SerializeField] private Image arrowLeft;
    [SerializeField] private Image tapTip;

    private int lives = 3;

    public Image Drill { get; private set; }
    public Animator Animator { get { return animator; } }
    public Vector2 Position { get { return Drill.rectTransform.anchoredPosition; } set { Drill.rectTransform.anchoredPosition = value; } }
    public Rigidbody2D Body { get; private set; }
    public enum Tile { ROCK, PIPE, BOMB, BOMB_AREA, DIAMOND, LIFE, ELECTRICITY, GROUND_TILE, WATER }
    public int Lives { get { return lives; } }
    public DrillGameHud Hud { get; private set; }
    public bool Collided { get; private set; }
    public Image ArrowDown { get { return arrowDown; } }
    public Image ArrowUp { get { return arrowUp; } }
    public Image ArrowRight { get { return arrowRight; } }
    public Image ArrowLeft { get { return arrowLeft; } }
    public Image TapTip { get { return tapTip; } }

    void Awake()
    {
        Drill = GetComponent<UnityEngine.UI.Image>();
        Body = GetComponent<Rigidbody2D>();
        Hud = FindObjectOfType<DrillGameHud>();
    }

    void Start()
    {
        Drill.gameObject.SetActive(false);
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
                handleCollision(Tile.LIFE, coll.gameObject);
                break;
        }
    }

    private void resetAnimation()
    {
        animator.SetBool("isSlidingLeft", false);
        animator.SetBool("isDrillingDown", false);
        animator.SetBool("isDrillingUp", false);
        animator.SetBool("isDrillingRight", false);
        animator.SetBool("isDrillingLeft", false);
        animator.SetBool("shouldJump", false);
        animator.SetBool("goToSliding", false);
    }

    public void Reset(Vector2 startPosition)
    {
        resetAnimation();
        if (!GameManager.Instance.DrillingGame.IsRestarting) lives = 3;
        Drill.rectTransform.anchoredPosition = startPosition;
        Drill.gameObject.SetActive(false);
        Collided = false;
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
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                if (lives == 3) updateDrillerLife(-3);
                else lives = 0;
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.EXPLODED_BOMB;
                Collided = true;
                break;
            case Tile.BOMB_AREA:
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_DRILL; //also TRIGGERED_BOMB available
                Collided = true;
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
                if (lives < 3)
                {
                    updateDrillerLife(1);
                    LeanTween.scale(Hud.DrillLife.GetComponent<RectTransform>(), Hud.DrillLife.GetComponent<RectTransform>().localScale * 1.1f, 0.8f)
                        .setEase(LeanTweenType.punch);
                }
                Destroy(GO);
                break;
            case Tile.PIPE:
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_PIPE;
                Collided = true;
                break;
            case Tile.ROCK:
                GameManager.Instance.Director.Shake(GameManager.Instance.DrillingGame.transform);
                updateDrillerLife(-1);
                GameManager.Instance.DrillingGame.ToastType = global::ToastType.BROKEN_DRILL;
                Collided = true;
                break;
            case Tile.GROUND_TILE:
                Destroy(GO);
                break;
            case Tile.WATER:
                GameManager.Instance.DrillingGame.Map.AddPipePart(GO.GetComponent<DrillingGameTile>());
                if (GameManager.Instance.DrillingGame.Map.GetPipePartsCount <= 3)
                    LeanTween.scale(Hud.WaterBar.GetComponent<RectTransform>(),
                        Hud.WaterBar.GetComponent<RectTransform>().localScale * 1.1f, 0.8f).setEase(LeanTweenType.punch);
                Destroy(GO);
                break;
        }
    }

    private void updateDrillerLife(int amount)
    {
        lives += amount;
    }

    public void ActivateImage(Image arrow, bool activate)
    {
        arrow.enabled = activate;
    }
}
