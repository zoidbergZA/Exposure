using UnityEngine;
using System.Collections;

public class Placable : MonoBehaviour
{
    public virtual void Awake()
    {
        
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public void Orientate(Vector3 normal)
    {
        transform.rotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
    }
}
