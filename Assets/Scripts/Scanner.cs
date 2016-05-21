using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float range = 200f;
    [SerializeField] private LayerMask scanRayMask;
    [SerializeField] private LeanTweenType TweenType;

    private float cooldownLeft;
    private Vector3 centerPoint;
    private Material material;
    private Renderer renderer;
    private float radius;
    
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

        StartScan();
    }

    void Update()
    {
        material.SetVector("_CenterPoint", new Vector4(centerPoint.x, centerPoint.y, centerPoint.z, 0));

        cooldownLeft -= Time.deltaTime;

        if (cooldownLeft <= 0)
        {
            cooldownLeft = cooldownTime;
            StartScan();
        }

        material.SetFloat("_Radius", radius);
    }

    private void StartScan()
    {
        IsScanning = true;

        Vector3 dir = (GameManager.Instance.PlanetTransform.position - Camera.main.transform.position).normalized;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, dir, out hit))
        {
            centerPoint = hit.point;
        }

        LeanTween.value(gameObject, radiusTweenCallback, 5f, range, duration)
            .setEase(TweenType)
            .setOnComplete(EndScan);
    }

    private void EndScan()
    {
        IsScanning = false;

        radius = 0;
    }

    void radiusTweenCallback(float val, float ratio)
    {
        radius = val;
    }
}
