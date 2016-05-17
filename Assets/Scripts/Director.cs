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
    private Vector3 targetPosition;
    private float orbitTimer;

    public Modes Mode { get; private set; }

	void Start ()
	{
	    distance = Vector3.Distance(transform.position, targetTransform.position);
	}
	
	void Update ()
    {
	    switch (Mode)
	    {
	        case Modes.Orbit:
	            orbitTimer += Time.deltaTime;
                targetPosition = new Vector3(Mathf.Sin(orbitTimer*orbitSpeed) * distance, targetTransform.position.y, Mathf.Cos(orbitTimer * orbitSpeed) * distance);
	            break;
	    }

	    transform.position = Vector3.Lerp(transform.position, targetPosition, 0.05f);
        transform.LookAt(GameManager.Instance.PlanetTransform);
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
                Camera.main.orthographicSize = buildZoom;
                targetPosition = targetTransform.position + targetTransform.up * 20f;
                break;

            case Modes.Orbit:
                Camera.main.orthographicSize = orbitZoom;
                break;
        }
    }
}
