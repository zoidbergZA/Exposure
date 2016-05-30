using UnityEngine;
using System.Collections;

public class Orbiter : MonoBehaviour
{
    [SerializeField] private Transform model;
    [SerializeField] private float speed = 2f;

	void Start ()
    {
	
	}

    void Update ()
    {
	    transform.RotateAround(GameManager.Instance.Planet.transform.position, Vector3.up, speed * Time.deltaTime);
        transform.LookAt(GameManager.Instance.Planet.transform);
	}
}
