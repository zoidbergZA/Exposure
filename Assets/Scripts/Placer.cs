using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Placer : MonoBehaviour
{
    public float rotationSpeed = 120.0f;
    public float translationSpeed = 10.0f;
    public float height = 0.5f;             //height from ground level
    private Transform centre;               //transform for planet
    private float radius;                   //calculated radius from collider
    //public SphereCollider planet;           //collider for planet
    public Collider planet;           //collider for planet
    private RaycastHit hit;
    private Vector3 prevPos;

    void Start()
    {
        //consider scale applied to planet transform (assuming uniform, just pick one)
        //radius = planet.radius * planet.transform.localScale.y;
        centre = planet.transform;
        transform.position = planet.transform.position * 24;
    }

    private bool hitsTarget(float fromDistance)
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, fromDistance) || Physics.Raycast(transform.position, transform.up, out hit, fromDistance) ||
            Physics.Raycast(transform.position, -transform.right, out hit, fromDistance) || Physics.Raycast(transform.position, transform.right, out hit, fromDistance) ||
            Physics.Raycast(transform.position, -transform.forward, out hit, fromDistance) || Physics.Raycast(transform.position, transform.forward, out hit, fromDistance))
        {
            if (hit.collider.Equals(planet))
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    void Update()
    {
        //snap position to radius + height (could also use raycasts)
        //Vector3 targetPosition = transform.position - centre.position;
        //float ratio = (radius + height) / targetPosition.magnitude;
        //targetPosition.Scale(new Vector3(ratio, ratio, ratio));
        //transform.position = targetPosition + centre.position;
        ////calculate planet surface normal                      
        //Vector3 surfaceNormal = transform.position - centre.position;
        //surfaceNormal.Normalize();
        ////align with surface normal
        //transform.rotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;

        if (hitsTarget(100))
        {
            // Stick on surface
            if((hit.point - centre.position).magnitude <= (transform.position - centre.position).magnitude)
            {
                transform.position = hit.point + hit.normal * height;
            }

            // Align to surface normal
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        //else transform.position = prevPos;
    }
}
