using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Director : MonoBehaviour
{
    public enum Modes
    {
        Orbit,
        Grid
    }

    public float buildHeight = 100f;
    public float normalZoom = 45f;
    public float buildzoom = 45f;

    [SerializeField] private Light sunLight;
    [SerializeField] private float sunDark;
    [SerializeField] private Shaker shaker;

    private Vector3 orbitPosition;
    private Quaternion orbitRotation;
    private float orbitHeight;

    //tweener values
    private Vector3 targetPosition;
    private Quaternion fromRotation;
    private Quaternion targetRotation;
    private float rotateProgress;
    private float targetFoV;
    private int positionTweenId;
    private int rotationTweenId;
    private int fovTweenId;
    private int sunlightTweenId;
    private float sunIntensity;
    private float sunBright;

    public bool OrbitPaused { get; set; }
    public Modes Mode { get; private set; }
    public bool IsPositionTweening { get { if (LeanTween.isTweening(positionTweenId)) return true; return false;} }

    void Awake()
    {
        orbitPosition = transform.position;
        orbitRotation = transform.rotation;
        targetFoV = Camera.main.fieldOfView;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        sunBright = sunLight.intensity;
    }

	void Start ()
	{
        orbitHeight = Vector3.Distance(transform.position, GameManager.Instance.PlanetTransform.position);
    }
	
	void Update ()
	{
	    transform.position = targetPosition;
	    Camera.main.fieldOfView = targetFoV;
	    sunLight.intensity = sunIntensity;
            
        if (LeanTween.isTweening(rotationTweenId))
            transform.rotation = Quaternion.Slerp(fromRotation, targetRotation, rotateProgress);
    }

    public void SetMode(Modes mode, Transform targetTransform, float delay = 2f)
    {
//        if (delay > 0)
//            StartCoroutine(DelayedStart(mode, targetTransform, delay));
//        else
            SwitchMode(mode, targetTransform, delay);
    }

    public void Shake()
    {
        shaker.Shake();
    }

    public void Shake(Transform other)
    {
        shaker.Shake(other);
    }

    public void SetSunlightBrightness(bool dark)
    {
        if (LeanTween.isTweening(sunlightTweenId))
            LeanTween.cancel(sunlightTweenId);

        float target = sunBright;
        if (dark)
            target = sunDark;

        sunlightTweenId = LeanTween.value(gameObject, updateSunlightCallback, sunLight.intensity, target, 1.2f).setEase(LeanTweenType.easeOutSine).id;
    }

    private IEnumerator DelayedStart(Modes newMode, Transform newTargetTransform, float delay)
    {
        yield return new WaitForSeconds(delay);

        SwitchMode(newMode, newTargetTransform);
    }

    private void SwitchMode(Modes newMode, Transform targetTransform, float delay = 2f)
    {
        Mode = newMode;

        switch (Mode)
        {
            case Modes.Grid:
//                GameManager.Instance.Planet.IsSpinning = false;

                Vector3 newPos = targetTransform.position + targetTransform.up * buildHeight + Vector3.down * 30f;
                Quaternion newRot = Quaternion.LookRotation(targetTransform.position - newPos, Vector3.up);

                SwoopTo(newPos, newRot, buildzoom, delay);
                break;

            case Modes.Orbit:
//                GameManager.Instance.Planet.IsSpinning = true;

                SwoopTo(orbitPosition, orbitRotation, normalZoom, delay);
                break;
        }
    }

    public void SwoopTo(Vector3 position, Quaternion rotation, float fov, float time, float delay = 0f)
    {
        fromRotation = transform.rotation;
        targetRotation = rotation;
        rotateProgress = 0f;

        if (LeanTween.isTweening(positionTweenId))
            LeanTween.cancel(positionTweenId);
        if (LeanTween.isTweening(fovTweenId))
            LeanTween.cancel(fovTweenId);

        positionTweenId = LeanTween.value(gameObject, updatePosCallback, targetPosition, position, time).setEase(LeanTweenType.easeInOutQuart).id;
        rotationTweenId = LeanTween.value(gameObject, updateRotCallback, 0f, 1f, time).setEase(LeanTweenType.easeInOutQuart).id;
        fovTweenId = LeanTween.value(gameObject, updateFOVCallback, targetFoV, fov, time).setEase(LeanTweenType.easeInOutSine).id;
    }

    void updateSunlightCallback(float val)
    {
        sunIntensity = val;
    }

    void updatePosCallback(Vector3 val)
    {
        targetPosition = val;
    }

    void updateRotCallback(float val)
    {
        rotateProgress = val;
    }

    void updateFOVCallback(float val)
    {
        targetFoV = val;
    }
}
