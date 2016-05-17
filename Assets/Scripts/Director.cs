using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    public enum Modes
    {
        Orbit,
        Grid
    }

    [SerializeField] private Transform targetTransform;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private float orbitZoom = 1f;
    [SerializeField] private float buildZoom = 0.8f;

    private float distance;
    private float currentZoom;
    private Vector3 targetPosition;
    private float orbitTimer;
    public bool OrbitPaused { get; set; }

    public Modes Mode { get; private set; }

	void Start ()
	{
	    distance = Vector3.Distance(transform.position, targetTransform.position);
	    currentZoom = orbitZoom;
	}
	
	void Update ()
    {
	    switch (Mode)
	    {
	        case Modes.Orbit:
                if(!OrbitPaused) orbitTimer += Time.deltaTime;
                targetPosition = new Vector3(Mathf.Sin(orbitTimer*orbitSpeed) * distance, targetTransform.position.y, Mathf.Cos(orbitTimer * orbitSpeed) * distance);
	            break;
	    }

	    transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
        transform.LookAt(GameManager.Instance.PlanetTransform);
	    Camera.main.orthographicSize = currentZoom;
    }

    public void SetMode(Modes mode, Transform targetTransform, float delay = 2f)
    {
//        if (delay > 0)
//            StartCoroutine(DelayedStart(mode, targetTransform, delay));
//        else
            SwitchMode(mode, targetTransform);
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        targetPosition = targetTransform.position + targetTransform.up * 20f;
    }

    public void LookAt(Vector3 position)
    {
        Camera.main.transform.LookAt(position);
    }

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
                Camera.main.orthographic = true;
                targetPosition = targetTransform.position + targetTransform.up * 20f;
                LeanTween.value(gameObject, updateValueExampleCallback, currentZoom, buildZoom, 1.1f).setEase(LeanTweenType.easeInOutSine);
                break;

            case Modes.Orbit:
                LeanTween.value(gameObject, updateValueExampleCallback, currentZoom, orbitZoom, 1.1f).setEase(LeanTweenType.easeInOutSine);
                break;
        }
    }

    void updateValueExampleCallback(float val, float ratio)
    {
        currentZoom = val;
    }
}
