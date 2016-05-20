using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
    [SerializeField] private float durationTime = 3f;
    [SerializeField] private LayerMask scanRayMask;
    private float cooldown;
    private float duration;
    public Transform testTarget;
    private Material material;
    private Renderer renderer;
    private bool scannedForth = false;

    public bool IsReady { get; private set; }
    public float Cooldown { get { return cooldown; } }

    public float Duration { get { return duration; } }

    void Awake()
    {
        IsReady = true;
    }

    void Start()
    {
        renderer = GameManager.Instance.Planet.scannableMesh.GetComponent<Renderer>();
        material = renderer.material;
//        material.SetFloat("_setDuration", durationTime);
    }

    void Update()
    {
        material.SetVector("_CenterPoint", new Vector4(testTarget.position.x, testTarget.position.y, testTarget.position.z, 0));

        //        if (!IsReady)
        //        {
        //            cooldown -= Time.deltaTime;
        //            duration -= Time.deltaTime;
        //
        //            if (duration >= 0)
        //            {
        //                if (!scannedForth) material.SetFloat("_Duration", duration);
        //                else material.SetFloat("_DurationBack", duration);
        //            }
        //            else
        //            {
        //                if (scannedForth == false)
        //                {
        //                    material.SetFloat("_Duration", 0);
        //                    duration = durationTime / 2;
        //                    scannedForth = true;
        //                }
        //            }
        //            if (cooldown <= 0)
        //            {
        //                IsReady = true;
        //                scannedForth = false;
        //            }
        //        }
    }

    public void Scan()
    {
        IsReady = false;
        cooldown = cooldownTime;
        duration = durationTime/2;
        getCenterPoint();
    }

    private void getCenterPoint()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); 
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanRayMask))
        {
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;
            material.SetFloat("_Radius",  hit.collider.bounds.size.x);
            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null) return;
            Vector3 targetFragment = hit.point;
            rend.material.SetVector("_CenterCoords", new Vector4(targetFragment.x, targetFragment.y, targetFragment.z, 0));
        }
    }
}
