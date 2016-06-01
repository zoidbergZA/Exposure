using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
    public MeshRenderer scannableMesh;

    [SerializeField] private Transform propsHolder;
    [SerializeField] private Transform citiesHolder;
    [SerializeField] private Transform globeTransform;
    [Range(0f, 1f)]
    [SerializeField] private float inertia;
    [SerializeField] private Light atmosphereLight;
    [SerializeField] private Color dirtyColor;
    [SerializeField] private Color cleanColor;
    [SerializeField] private float normalSpin;

    private float momentum;
    private float currentSpin;
    private Chimney[] chimneys;

    public float Health { get; private set; }
    public bool IsSpinning { get; set; }

    void Awake()
    {
        chimneys = FindObjectsOfType<Chimney>();
        currentSpin = normalSpin;

        propsHolder.SetParent(transform);
        citiesHolder.SetParent(transform);

        RefreshHealth();
    }

    void Update()
    {
        HandleSpin();

        //test spin input
        if (Input.GetKeyDown(KeyCode.A))
        {
            momentum += 80;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            momentum -= 80;
        }
    }

    public void AddSpin(float amount)
    {
        momentum += amount;
    }

    public void RefreshHealth()
    {
        int unusedCount = 0;

        for (int i = 0; i < chimneys.Length; i++)
        {
            if (chimneys[i].ChimneyState != Chimney.ChimneyStates.Working)
            {
                unusedCount++;
            }
        }

        Health = (float)unusedCount / (float)chimneys.Length;
        atmosphereLight.color = Color.Lerp(dirtyColor, cleanColor, Health);
//        Debug.Log("planet health: " + Health);
    }

    public void DisableNextChimney()
    {
        foreach (Chimney chimney in chimneys)
        {
            if (chimney.ChimneyState == Chimney.ChimneyStates.Working)
            {
                chimney.DisableChimney();
                chimney.Demolish();
                return;
            }
        }
    }

    private void HandleSpin()
    {
        if (!IsSpinning)
            return;

        momentum *= inertia;

        currentSpin = Mathf.Lerp(currentSpin, normalSpin, inertia) + momentum;


        transform.Rotate(Vector3.up, currentSpin * Time.deltaTime);
    }
}
