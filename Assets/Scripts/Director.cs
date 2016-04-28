using UnityEngine;
using System.Collections;

public class Director : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float degreesPerSecond = 10f;

    private float distance;

	void Start ()
	{
	    distance = Vector3.Distance(transform.position, targetTransform.position);
	}
	
	void Update ()
    {
	    transform.RotateAround(targetTransform.position, Vector3.up, degreesPerSecond * Time.deltaTime);
	}
}
