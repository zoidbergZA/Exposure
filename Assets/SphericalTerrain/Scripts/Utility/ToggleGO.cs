using UnityEngine;
using System.Collections;

public class ToggleGO : MonoBehaviour
{
	public string key = "m";
	public GameObject go;

	private bool toggle = true;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(key))
		{
			toggle = !toggle;
		}

		if (toggle)
		{
			go.SetActive(true);
		}
		else
		{
			go.SetActive(false);
		}
	}
}
