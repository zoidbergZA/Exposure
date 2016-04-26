using UnityEngine;
using System.Collections;

public class OceanController : MonoBehaviour
{
    Vector3 scale = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (transform.localScale.x != PlanetStatistics.waterLevel)
        {
            scale.x = PlanetStatistics.waterLevel;
            scale.y = PlanetStatistics.waterLevel;
            scale.z = PlanetStatistics.waterLevel;

            transform.localScale = scale;
        }
	}
}
