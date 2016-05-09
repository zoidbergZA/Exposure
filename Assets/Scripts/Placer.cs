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
    public SphereCollider planet;           //collider for planet
    //public MeshCollider planet;           //collider for planet

    void Start()
    {
        transform.position = new Vector3(1, 1, 1);
        //consider scale applied to planet transform (assuming uniform, just pick one)
        radius = planet.radius * planet.transform.localScale.y;
        centre = planet.transform;
    }

    void Update()
    {
        //translate based on input     
        float inputMag = Input.GetAxis("Vertical") * translationSpeed * Time.deltaTime;
        transform.position += transform.forward * inputMag;
        //snap position to radius + height (could also use raycasts)
        Vector3 targetPosition = transform.position - centre.position;
        float ratio = (radius + height) / targetPosition.magnitude;
        targetPosition.Scale(new Vector3(ratio, ratio, ratio));
        transform.position = targetPosition + centre.position;
        //calculate planet surface normal                      
        Vector3 surfaceNormal = transform.position - centre.position;
        surfaceNormal.Normalize();
        //GameObject's heading
        float headingDeltaAngle = Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed;
        Quaternion headingDelta = Quaternion.AngleAxis(headingDeltaAngle, transform.up);
        //align with surface normal
        transform.rotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        //apply heading rotation
        transform.rotation = headingDelta * transform.rotation;


        //RaycastHit hit;

        //if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity))
        //{
        //    // Stick on surface
        //    transform.position = hit.point + hit.normal * height;

        //    // Align to surface normal
        //    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        //}
    }
}
