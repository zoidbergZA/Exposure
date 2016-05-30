using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    public enum Modes
    {
        Orbit,
        Grid
    }

    private Transform targetTransform;
    [SerializeField] private Shaker shaker;
    [SerializeField] private Vector3 lookAtOffset;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private float orbitZoom = 1f;
    [SerializeField] private float buildZoom = 0.8f;

    private float distance;
//    private float currentZoom;
    private Vector3 targetPosition;
    private float targetFoV;
    private float orbitTimer;
    public bool OrbitPaused { get; set; }

    public Modes Mode { get; private set; }

    void Awake()
    {
        targetFoV = 40f;
    }

	void Start ()
	{
	    targetTransform = GameManager.Instance.PlanetTransform;
	    distance = Vector3.Distance(transform.position, targetTransform.position);
        
//	    currentZoom = orbitZoom;
	}
	
	void Update ()
    {
//	    switch (Mode)
//	    {
//	        case Modes.Orbit:
//                if(!OrbitPaused) orbitTimer += Time.deltaTime;
//                targetPosition = new Vector3(Mathf.Sin(orbitTimer*orbitSpeed) * distance, targetTransform.position.y, Mathf.Cos(orbitTimer * orbitSpeed) * distance);
//                transform.LookAt(GameManager.Instance.PlanetTransform.position + Camera.main.transform.right * lookAtOffset.x);
//                break;
//
//            case Modes.Grid:
//                transform.LookAt(GameManager.Instance.PlanetTransform.position);
//                break;
//	    }
//
//	    transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
	    Camera.main.fieldOfView = targetFoV;

//        if (Input.GetKeyDown(KeyCode.S))
//            shaker.Shake();
    }

    public void SetMode(Modes mode, Transform targetTransform, float delay = 2f)
    {
//        if (delay > 0)
//            StartCoroutine(DelayedStart(mode, targetTransform, delay));
//        else
            SwitchMode(mode, targetTransform);
    }

    public void Shake()
    {
        shaker.Shake();
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        targetPosition = targetTransform.position + targetTransform.up * distance;
    }

//    public void LookAt(Vector3 position)
//    {
//        Camera.main.transform.LookAt(position);
//    }

    private IEnumerator DelayedStart(Modes newMode, Transform newTargetTransform, float delay)
    {
        yield return new WaitForSeconds(delay);

        SwitchMode(newMode, newTargetTransform);
    }

    private void SwitchMode(Modes newMode, Transform newTargetTransform)
    {
        Mode = newMode;
        targetTransform = newTargetTransform;

        switch (Mode)
        {
            case Modes.Grid:
//                Camera.main.orthographic = true;
//                Camera.main.fieldOfView = 20f;
                GameManager.Instance.Planet.IsSpinning = false;
                targetPosition = targetTransform.position + targetTransform.up * distance;
                LeanTween.value(gameObject, updateValueExampleCallback, targetFoV, 20f, 1.1f).setEase(LeanTweenType.easeInOutSine);
                break;

            case Modes.Orbit:
                //Camera.main.orthographic = false;
                //                Camera.main.fieldOfView = 40f;
                GameManager.Instance.Planet.IsSpinning = true;
                LeanTween.value(gameObject, updateValueExampleCallback, targetFoV, 40f, 1.1f).setEase(LeanTweenType.easeInOutSine);
                break;
        }
    }

    void updateValueExampleCallback(float val, float ratio)
    {
        targetFoV = val;
    }
}
