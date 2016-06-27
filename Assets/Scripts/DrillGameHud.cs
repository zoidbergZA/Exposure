using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DrillGameHud : MonoBehaviour
{
    public float JoystickArrowFadeSpeed = 2f;
    public float ToastMessageTime = 3.0f;
    public float PanelSlidingTime = 1.5f;
    [SerializeField] private Image joystickArrow;
    [SerializeField] private Image brokenDrillToast;
    [SerializeField] private Image succeededToast;
    [SerializeField] private Image succeededToastFemale;
    [SerializeField] private Image timeoutToast;
    [SerializeField] private Image pipeProgressBar;
    [SerializeField] private Image drillLife;
    [SerializeField] private Image waterLeft;
    [SerializeField] private Image waterRight;
    [SerializeField] private Image waterBottom;
    [SerializeField] private Image waterHot;
    [SerializeField] private Image waterSteam;
    [SerializeField] private Text debugSpeed;

    public Image JoystickArrow { get { return joystickArrow; } }
    public Image BrokenDrillToast { get { return brokenDrillToast; } }
    public Image WaterBar { get { return pipeProgressBar; } }
    public Image DrillLife { get { return drillLife; } }
    public float ToastTimer { get; set; }
    public float PanelSlidingTimer { get; set; }
    public DrillGameMap Map { get; private set; }
    public Driller Driller { get; private set; }
    public bool ActivateGeothermalUI { get; set; }
    public bool DeactivateGeoThermalUI { get; set; }

    private Vector2 waterLeftStartPosition = new Vector2(0, 688);
    private Vector2 waterRightStartPosition = new Vector2(0, -683);
    private Vector2 waterBottomStartPosition = new Vector2(-1114, 0);
    private Vector2 waterHotStartPosition = new Vector2(0, -46);
    private Vector2 waterSteamStartPosition = new Vector2(0, -90);

    void Awake()
    {
        Map = FindObjectOfType<DrillGameMap>();
        Driller = FindObjectOfType<Driller>();
    }

	void Start ()
    {
        brokenDrillToast.gameObject.SetActive(false);
        succeededToast.gameObject.SetActive(false);
        succeededToastFemale.gameObject.SetActive(false);
        ToastTimer = ToastMessageTime;
        PanelSlidingTimer = PanelSlidingTime;
	}
	
	void Update ()
    {
        if (joystickArrow.color.a > 0 && GameManager.Instance.DrillingGame.State == DrillingGame.DrillingGameState.DRILLING)
        {
            joystickArrow.gameObject.SetActive(true);
            joystickArrow.color = new Color(1, 1, 1, joystickArrow.color.a - Time.deltaTime * JoystickArrowFadeSpeed);
        }
        if (GameManager.Instance.DrillingGame.State != DrillingGame.DrillingGameState.DRILLING)
        {
            joystickArrow.color = new Color(1, 1, 1, 0);
        }
        updateProgressBars();
        if (ActivateGeothermalUI) ActivateGeothermal(true);
        if (DeactivateGeoThermalUI) ActivateGeothermal(false);

        //debug speed update here
        debugSpeed.text = "Speed\n" + GameManager.Instance.DrillingGame.DrillSpeed / 10 + "\n" + GameManager.Instance.DrillingGame.Joystick.DebugText;
    }
    public void PointJoystickArrow(DrillingDirection direction)
    {
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
        switch (toastType)
        {
            case global::ToastType.BROKEN_DRILL:
                brokenDrillToast.gameObject.SetActive(false);
                break;
            case global::ToastType.TIME_OUT:
                timeoutToast.gameObject.SetActive(false);
                break;
            case global::ToastType.EXPLODED_BOMB:
                brokenDrillToast.gameObject.SetActive(false);
                break;
            case global::ToastType.SUCCESS:
                succeededToast.gameObject.SetActive(false);
                succeededToastFemale.gameObject.SetActive(false);
                DeactivateGeoThermalUI = true;
                break;
        }
        ToastTimer = ToastMessageTime;
    }

    private void updateProgressBars()
    {
        if (pipeProgressBar && Map.GetPipePartsCount <= 3) pipeProgressBar.fillAmount = Map.GetPipePartsCount * 33.33333334f / 100f;
        switch(Driller.Lives)
        {
            case 3:
                drillLife.fillAmount = 1.00f;
                break;
            case 2:
                drillLife.fillAmount = 0.66666669f;
                break;
            case 1:
                drillLife.fillAmount = 0.33333339f;
                break;
            default:
                drillLife.fillAmount = 0.00f;
                break;
        }
    }

    public void ActivateToast(ToastType type)
    {
        switch (type)
        {
            case global::ToastType.TIME_OUT:
                if (!timeoutToast.gameObject.activeSelf) timeoutToast.gameObject.SetActive(true);
                timeoutToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.BROKEN_DRILL:
                if (!brokenDrillToast.gameObject.activeSelf) brokenDrillToast.gameObject.SetActive(true);
                brokenDrillToast.gameObject.transform.parent.SetAsLastSibling();
                break;
            case global::ToastType.EXPLODED_BOMB:
                //replace with appropriate image when ready
                drillLife.fillAmount = 0.00f;
                break;
            case global::ToastType.TRIGGERED_BOMB:
                //replace with appropriate image when ready
                drillLife.fillAmount = 0.00f;
                break;
            case global::ToastType.SUCCESS:
                if (!succeededToast.gameObject.activeSelf && Driller.Gender == Driller.DrillerGender.MALE)
                {
                    succeededToast.gameObject.SetActive(true);
                    succeededToast.gameObject.transform.parent.SetAsLastSibling();
                }
                if (!succeededToastFemale.gameObject.activeSelf && Driller.Gender == Driller.DrillerGender.FEMALE)
                {
                    succeededToastFemale.gameObject.SetActive(true);
                    succeededToastFemale.gameObject.transform.parent.SetAsLastSibling();
                }
                break;
        }
    }

    public void Reset()
    {
        pipeProgressBar.fillAmount = 0f;
        if (!GameManager.Instance.DrillingGame.IsRestarting) drillLife.fillAmount = 0.0f;
    }

    public void ActivateGeothermal(bool activate)
    {
        if(activate)
        {
            LeanTween.move(waterLeft.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutQuart);
            if (waterLeft.gameObject.GetComponent<RectTransform>().anchoredPosition.y <= 5.0f)
            {
                LeanTween.move(waterBottom.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutQuart);
                if (waterBottom.gameObject.GetComponent<RectTransform>().anchoredPosition.x >= -5.0f)
                {
                    LeanTween.move(waterRight.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutQuart);
                    if (waterRight.gameObject.GetComponent<RectTransform>().anchoredPosition.y >= -5.0f)
                    {
                        LeanTween.move(waterHot.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutQuart);
                        if (waterHot.gameObject.GetComponent<RectTransform>().anchoredPosition.y >= -5.0f)
                        {
                            LeanTween.move(waterSteam.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutQuart);
                            if (waterSteam.gameObject.GetComponent<RectTransform>().anchoredPosition.y >= -5.0f)
                            {
                                ActivateGeothermalUI = false;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            LeanTween.move(waterLeft.gameObject.GetComponent<RectTransform>(), waterLeftStartPosition, ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(waterRight.gameObject.GetComponent<RectTransform>(), waterRightStartPosition, ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(waterBottom.gameObject.GetComponent<RectTransform>(), waterBottomStartPosition, ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(waterHot.gameObject.GetComponent<RectTransform>(), waterHotStartPosition, ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(waterSteam.gameObject.GetComponent<RectTransform>(), waterSteamStartPosition, ToastMessageTime).setEase(LeanTweenType.easeOutQuad);
            DeactivateGeoThermalUI = false;
        }
    }
}
