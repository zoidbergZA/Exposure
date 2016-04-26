using UnityEngine;
using System.Collections;

public class RotateAround : MonoBehaviour {

	public Transform target;

	public Vector3 direction = new Vector3(0, 0, 0);

	public float speed = 10;

	// Use this for initialization
	void Start ()
	{
		//target = GameObject.Find ("Globe").transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.RotateAround(target.position, direction, speed * Time.deltaTime);
	}
}
