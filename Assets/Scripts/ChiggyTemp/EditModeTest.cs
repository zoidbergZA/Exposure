using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EditModeTest : MonoBehaviour
{
    public GameObject target;
    private float targetRadius;

    void Start()
    {
        targetRadius = target.GetComponent<SphereCollider>().radius;
    }

	void Update ()
    {
        if (target)
        {
            transform.LookAt(target.transform);
            snapToGlobe();
        }
	}

    private float squared(float value)
    {
        return value * value;
    }

    private void snapToGlobe()
    {
        if(squared(transform.position.x - target.transform.position.x) > targetRadius)
        {
            transform.position = new Vector3((target.transform.position.x + targetRadius), transform.position.y, transform.position.z);
        }
    }
}
