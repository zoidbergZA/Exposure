﻿using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private LayerMask scanRayMask;
    private float cooldown;
    public GameObject target;
    private Material material;
    private Renderer renderer;

    public bool IsReady { get; private set; }
    public float Cooldown { get { return cooldown; } }

    void Awake()
    {
        IsReady = true;
    }

    void Start()
    {
        renderer = target.GetComponent<Renderer>();
        material = renderer.material;
    }

    void Update()
    {
        if (!IsReady)
        {
            cooldown -= Time.deltaTime;

            material.SetFloat("_ScanFactor", Mathf.Sin(Time.time));
            if (cooldown <= 0)
            {
                IsReady = true;
                material.SetFloat("_ScanFactor", 1);
            }
        }
    }

    public void Scan()
    {
        IsReady = false;
        cooldown = cooldownTime;

        getCenterPoint();
    }

    private void getCenterPoint()
    {
        //Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); 
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            {
                return;
            }

            Texture2D tex = rend.material.mainTexture as Texture2D; //access to texture

            Vector2 pixelUV = hit.textureCoord; //access to texture uvs
            rend.material.SetVector("_CenterCoords", new Vector4(pixelUV.x, pixelUV.y, 0, 0));
            tex.Apply();
        }
    }
}
