using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Placer : MonoBehaviour
{
    public float height = 0.5f;     //height from ground level
    public Collider planet;         //collider for planet
    private RaycastHit hit;

    private Vector3 getDirection()
    {
        return (planet.transform.position - transform.position).normalized;
    }

    private bool hitsTarget(float fromDistance)
    {
        if (Physics.Raycast(transform.position, getDirection(), out hit, fromDistance))
        {
            if (hit.collider.Equals(planet)) return true;
            else return false;
        }
        else return false;
    }

    void Update()
    {
        if (hitsTarget(100))
        {
            transform.position = hit.point + hit.normal * height; // Stick on surface
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation; // Align to surface normal
        }
    }
}
