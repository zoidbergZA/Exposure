﻿using UnityEngine;
using System.Collections;

public class DrillGameHud : MonoBehaviour
{
    public float JoystickArrowFadeSpeed = 2f;
    public float ToastMessageTime = 3.0f;
    public float PanelSlidingTime = 1.5f;
    [SerializeField] private UnityEngine.UI.Image joystickArrow;
    [SerializeField] private UnityEngine.UI.Image endOkToast;
    [SerializeField] private UnityEngine.UI.Image brokenDrillToast;
    [SerializeField] private UnityEngine.UI.Image brokenPipeToast;
    [SerializeField] private UnityEngine.UI.Image explodeBombToast;
    [SerializeField] private UnityEngine.UI.Image waterBar;
    [SerializeField] private UnityEngine.UI.Image steamImage;
    [SerializeField] private UnityEngine.UI.Image drillLife;

    public UnityEngine.UI.Image JoystickArrow { get { return joystickArrow; } }
    public UnityEngine.UI.Image EndOkToast { get { return endOkToast; } }
    public UnityEngine.UI.Image BrokenDrillToast { get { return brokenDrillToast; } }
    public UnityEngine.UI.Image BrokenPipeToast { get { return brokenPipeToast; } }
    public UnityEngine.UI.Image WaterBar { get { return waterBar; } }
    public UnityEngine.UI.Image SteamImage { get { return steamImage; } }
    public UnityEngine.UI.Image DrillLife { get { return drillLife; } }
    public UnityEngine.UI.Image ExplodeBombToast { get { return explodeBombToast; } }
    public float ToastTimer { get; set; }
    public float PanelSlidingTimer { get; set; }
    public DrillGameMap Map { get; private set; }

    void Awake()
    {
        Map = FindObjectOfType<DrillGameMap>();
    }

	void Start ()
    {
        brokenDrillToast.gameObject.SetActive(false);
        endOkToast.gameObject.SetActive(false);
        ToastTimer = ToastMessageTime;
        PanelSlidingTimer = PanelSlidingTime;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //joystick arrow
        if (joystickArrow.color.a > 0)
        {
            joystickArrow.color = new Color(1, 1, 1, joystickArrow.color.a - Time.deltaTime * JoystickArrowFadeSpeed);
            joystickArrow.rectTransform.localPosition = GameManager.Instance.DrillingGame.Driller.Drill.rectTransform.localPosition;
        }
        updateProgressBars();
    }
    public void PointJoystickArrow(DrillingDirection direction)
    {
        joystickArrow.color = new Color(1, 1, 1, 1);

        float rotation = 0;

        switch (direction)
        {
            case DrillingDirection.UP:
                rotation = 0;
                break;
            case DrillingDirection.DOWN:
                rotation = 180;
                break;
            case DrillingDirection.LEFT:
                rotation = 90;
                break;
            case DrillingDirection.RIGHT:
                rotation = 270;
                break;
        }

        joystickArrow.transform.localEulerAngles = new Vector3(0, 0, rotation);
        joystickArrow.transform.SetAsLastSibling();
    }

    public void DeactivateToast(ToastType toastType)
    {
        ToastTimer = ToastMessageTime;
        switch (toastType)
        {
            case global::ToastType.BROKEN_DRILL:
                brokenDrillToast.gameObject.SetActive(false);
                break;
            case global::ToastType.BROKEN_PIPE:
                brokenPipeToast.gameObject.SetActive(false);
                break;
            case global::ToastType.EXPLODED_BOMB:
                //todo
                break;
            case global::ToastType.SUCCESS:
                endOkToast.gameObject.SetActive(false);
                LeanTween.move(steamImage.gameObject.GetComponent<RectTransform>(), new Vector3(0, -475, 0), ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
                break;
        }
        //drillLife.fillAmount = 0f;
        GameManager.Instance.DrillingGame.State = DrillingGame.DrillingGameState.INACTIVE;
    }

    private void updateProgressBars()
    {
        if (waterBar && Map.GetWaterCount <= 3) waterBar.fillAmount = Map.GetWaterCount * 33.33333334f / 100f;
        //to do update drill life depending on amount of life
        drillLife.fillAmount = 1.00f;
    }

    public void ActivateToast(ToastType type)
    {
        switch (type)
        {
            case global::ToastType.BROKEN_PIPE:
                brokenPipeToast.gameObject.SetActive(true);
                brokenPipeToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.BROKEN_DRILL:
                brokenDrillToast.gameObject.SetActive(true);
                brokenDrillToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.EXPLODED_BOMB:
                //todo
                break;
            case global::ToastType.TRIGGERED_BOMB:
                //todo
                break;
            case global::ToastType.SUCCESS:
                endOkToast.gameObject.SetActive(true);
                endOkToast.gameObject.transform.parent.SetAsLastSibling();
                break;
        }
    }
}
