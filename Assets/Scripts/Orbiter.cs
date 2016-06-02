using UnityEngine;
using System.Collections;

public class Orbiter : MonoBehaviour
{
    [SerializeField] private Transform model;
    [SerializeField] private float speed = 2f;
//    [SerializeField] private float spinSpeed;

    private Vector3 axis;
    private Vector3 oldPosition;

	void Start ()
	{
	    oldPosition = transform.position;
	    axis = Random.insideUnitSphere;
	}

    void Update ()
    {
	    transform.RotateAround(GameManager.Instance.Planet.transform.position, axis, speed * Time.deltaTime);

//        if (spinSpeed > 0)
//        {
//            transform.Rotate(0, spinSpeed, 0);
//        }

        Vector3 forward = transform.position + (transform.position - oldPosition) * 50f;
 
        model.LookAt(forward, transform.position - GameManager.Instance.PlanetTransform.position);

        oldPosition = transform.position;
    }
}
