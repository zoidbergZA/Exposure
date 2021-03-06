﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Placer : MonoBehaviour
{
    public float height = 0.5f;     //height from ground level
//    public Collider planet;         //collider for planet
    private RaycastHit hit;

    private Collider planetCollider;

    private Vector3 getDirection(Transform planet)
    {
        return (planet.position - transform.position).normalized;
    }

    private bool hitsTarget(float fromDistance)
    {
        if (Physics.Raycast(transform.position, getDirection(planetCollider.transform), out hit, fromDistance))
        {
            if (hit.collider.Equals(planetCollider))
                return true;
            else
                return false;
        }
        else return false;
    }
    
    void Update()
    {
        //todo: dont execute if not in editor mode

        if (!planetCollider)
        {
            planetCollider = GameObject.FindGameObjectWithTag("Planet").GetComponent<Collider>();
        }

        if (planetCollider && hitsTarget(100))
        {
            transform.position = hit.point + hit.normal * height; // Stick on surface
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation; // Align to surface normal
        }
    }
}
