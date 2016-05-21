using System;
using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    public ScanProperties smallScan;
    public ScanProperties globalScan;
     
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private LayerMask scanRayMask;

    private float cooldownLeft;
    private Vector3 centerPoint;
    private Material material;
    private Renderer renderer;
    private float radius;
    private int smallScanId;

    public bool IsScanning { get; private set; }
//    public float ScanProgress { get { return 1f - durationLeft/durationTime; } }
    public float Cooldown { get { return cooldownLeft; } }
    
    void Awake()
    {
        cooldownLeft = cooldownTime;
    }

    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;

//        StartScan(smallScan);
    }

    void Update()
    {
        material.SetVector("_CenterPoint", new Vector4(centerPoint.x, centerPoint.y, centerPoint.z, 0));

        cooldownLeft -= Time.deltaTime;

        if (cooldownLeft <= 0)
        {
            cooldownLeft = cooldownTime;
//            StartScan(globalScan);    //todo: global scans removed for testing local
        }

        material.SetFloat("_Radius", radius);
    }

    public void StartScan(ScanProperties scanProperties)
    {
        IsScanning = true;

        Vector3 dir = (GameManager.Instance.PlanetTransform.position - Camera.main.transform.position).normalized;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, dir, out hit))
        {
            centerPoint = hit.point;
        }

        LeanTween.value(gameObject, radiusTweenCallback, 5f, scanProperties.range, scanProperties.duration)
            .setEase(scanProperties.tweenType)
            .setOnComplete(EndScan);
    }

    public void StartScan(Vector3 point)
    {
        IsScanning = true;

        centerPoint = point;

        smallScanId = LeanTween.value(gameObject, radiusTweenCallback, 5f, smallScan.range, smallScan.duration)
            .setEase(smallScan.tweenType)
            .setOnComplete(EndScan)
            .id;
    }

    public void EndScan()
    {
        IsScanning = false;

        //todo: if tweening, cancel tween, call EndScan() when drilling game preloader cancels
        if (LeanTween.isTweening(smallScanId))
        {
            LeanTween.cancel(smallScanId);
        }

        radius = 0;
    }

    void radiusTweenCallback(float val, float ratio)
    {
        radius = val;
    }
}

[Serializable]
public class ScanProperties
{
    public float duration;
    public float range;
    public float fade;
    public LeanTweenType tweenType;
}
