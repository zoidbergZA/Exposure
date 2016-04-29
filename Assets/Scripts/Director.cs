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

    private float distance;
    private Vector3 targetPosition;

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
                targetPosition = new Vector3(Mathf.Sin(Time.time*orbitSpeed) * distance, targetTransform.position.y, Mathf.Cos(Time.time * orbitSpeed) * distance);
	            break;
	    }

	    transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
        transform.LookAt(targetTransform);
    }

    public void SetMode(Modes mode, Transform targetTransform)
    {
        Mode = mode;
        this.targetTransform = targetTransform;

        switch (Mode)
        {
            case Modes.Grid:
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = 3f;
                targetPosition = targetTransform.position + targetTransform.up*20f;
                break;
        }
    }

//    public void SetTarget(Transform target)
//    {
//        targetTransform = target;
//    }
}
