using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
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
    }
}
