using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private float durationTime = 3f;
    [SerializeField] private float range = 200f;
    [SerializeField] private LayerMask scanRayMask;

    private float cooldownLeft;
    private float durationLeft;
    public Transform testTarget;
    private Material material;
    private Renderer renderer;
    
    public bool IsScanning { get; private set; }
    public float ScanProgress { get { return 1f - durationLeft/durationTime; } }
    public float Cooldown { get { return cooldownLeft; } }
    
    void Awake()
    {
        cooldownLeft = cooldownTime;
    }

    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;
    }

    void Update()
    {
        material.SetVector("_CenterPoint", new Vector4(testTarget.position.x, testTarget.position.y, testTarget.position.z, 0));

        cooldownLeft -= Time.deltaTime;

        if (cooldownLeft <= 0)
        {
            cooldownLeft = cooldownTime;
            StartScan();
        }

        if (IsScanning)
        {
            durationLeft -= Time.deltaTime;
            
            if (durationLeft <= 0)
            {
                durationLeft = 0;
                EndScan();
            }
            else
            {
                HandleScanning();
            }
        }
    }

    private void StartScan()
    {
        Debug.Log("scan called at: " + Time.time);

        IsScanning = true;
        durationLeft = durationTime;
    }

    private void HandleScanning()
    {
        Debug.Log("progress: " + ScanProgress);

        material.SetFloat("_Radius", ScanProgress * range);
    }

    private void EndScan()
    {
        IsScanning = false;

        material.SetFloat("_Radius", 0f);
    }
}
