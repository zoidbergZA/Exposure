using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
	public float moveSpeed = 5;
	public float rotationSpeed = 2;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		MovePlayer (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
	}

	void MovePlayer (float horizontal, float vertical)
	{
		transform.rotation *= Quaternion.Euler(new Vector3 (0, horizontal * rotationSpeed, 0));

		transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);
	}
}
