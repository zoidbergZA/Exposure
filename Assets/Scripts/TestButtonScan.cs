using UnityEngine;
using System.Collections;

public class TestButtonScan : MonoBehaviour
{
    public GameObject target;
    private Material material;
    private Renderer renderer;
    private float lastScannedAt = 0.0f;
    public float scanCooldown = 7.0f;
    private bool scanning = false;

    void Start()
    {
        renderer = target.GetComponent<Renderer>();
        material = renderer.material;
        lastScannedAt = Time.time;
    }
    public void onClick()
    {
        scanGlobe();
    }

    void Update()
    {
        updateMaterial();
    }

    private void scanGlobe()
    {
        if(Time.time - lastScannedAt >= scanCooldown)
        {
            lastScannedAt = Time.time;
            scanning = true;
        }
    }

    private void updateMaterial()
    {
        if (scanning)
        {
            if(Time.time - lastScannedAt < scanCooldown)
            {
                material.SetFloat("_ScanFactor", Mathf.Sin(Time.time));
            }
            else
            {
                material.SetFloat("_ScanFactor", 1);
                scanning = false;
            }
        }
    }
}
