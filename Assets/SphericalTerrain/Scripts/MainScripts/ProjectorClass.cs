using UnityEngine;
using System.Collections;

public class ProjectorClass : MonoBehaviour
{
	private Transform planetTransform;
	private Transform p_transform;
	private float p_size;

	private RaycastHit hit;
	private Ray ray;

	void Start ()
	{
		planetTransform = transform;
		p_transform = planetTransform.FindChild("Projector");
	}

	void Update ()
	{
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		// Projector handler
		if (Physics.Raycast (ray, out hit, Mathf.Infinity))
		{
			p_transform.LookAt(planetTransform);
			p_transform.position = hit.point * 1.5f;
		}
	}
}
