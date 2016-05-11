using UnityEngine;
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

            //material.SetFloat("_ScanFactor", Mathf.Sin(Time.time));
            if (cooldown <= 0)
            {
                IsReady = true;
                material.SetFloat("_Cooldown", cooldown);
            }
        }
    }

    public void Scan()
    {
        IsReady = false;
        cooldown = cooldownTime;
        material.SetFloat("_Cooldown", cooldown);
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
            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null) return;
            Vector3 targetFragment = hit.point;
            rend.material.SetVector("_CenterCoords", new Vector4(targetFragment.x, targetFragment.y, targetFragment.z, 0));
        }
    }
}
