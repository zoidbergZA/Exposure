using UnityEngine;
using System.Collections;

public static class PlanetHelpers
{
	public static float GetAltitude (Transform tr1, Transform tr2)
    {
        float altitude = 0;

        altitude = Vector3.Distance(tr1.position, tr2.position);

        return altitude;
    }
}
