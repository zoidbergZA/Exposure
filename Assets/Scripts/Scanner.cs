using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 7f;
    private float cooldown;

    public bool IsReady { get; private set; }
    public float Cooldown { get { return cooldown; } }

    void Awake()
    {
        IsReady = true;
    }

    void Update()
    {
        if (!IsReady)
        {
            cooldown -= Time.deltaTime;

            if (cooldown <= 0)
            {
                IsReady = true;
            }
        }
    }

    public void Scan()
    {
        IsReady = false;
        cooldown = cooldownTime;

        // todo: Add scanner shader logic here
    }
}
