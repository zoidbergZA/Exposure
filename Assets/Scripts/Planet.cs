using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
    public MeshRenderer scannableMesh;
    public float normalSpin;

    [SerializeField] private Transform propsHolder;
    [SerializeField] private Transform citiesHolder;
    [SerializeField] private Transform globeTransform;
    [Range(0f, 1f)]
    [SerializeField] private float inertia;
    [Range(0f, 1f)]
    [SerializeField] private float startTrees;
    [SerializeField] private Light atmosphereLight;
    [SerializeField] private Color dirtyColor;
    [SerializeField] private Color cleanColor;
    [SerializeField] private ParticleSystem cloudParticleSystem;
    [SerializeField] private Color cloudsDirtyColor;
    [SerializeField] private Color cloudsCleanColor;
    [SerializeField] private MeshRenderer waterMeshRenderer;
    [SerializeField] private Color waterDirtyColor;
    [SerializeField] private Color waterCleanColor;
    [SerializeField] private Color backgroundDirtyColor;
    [SerializeField] private Color backgroundCleanColor;

    private Material landMaterial;
    private float momentum;
    private float currentSpin;
    private Chimney[] chimneys;
    private Tree[] trees;
    int healthyTreesAtStart;

    public float Health { get; private set; }
    public bool IsSpinning { get { if (Mathf.Abs(momentum) > 1f) return true; return false;} }

    void Awake()
    {
        chimneys = FindObjectsOfType<Chimney>();
        trees = FindObjectsOfType<Tree>();
        InitializeTrees();
        currentSpin = normalSpin;

        propsHolder.SetParent(transform);
        citiesHolder.SetParent(transform);
    }

    void Start()
    {
        landMaterial = scannableMesh.GetComponent<Renderer>().material;
        RefreshHealth();
    }

    void Update()
    {
//        if (GameManager.Instance.RoundStarted)
            HandleSpin();

        //test spin input
        if (Input.GetKey(KeyCode.A))
        {
            momentum += 280*Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            momentum -= 280 * Time.deltaTime;
        }
    }

    public void AddSpin(float amount)
    {
        momentum += amount;
    }

    public void RefreshHealth()
    {
        int cleanCount = 0;

        for (int i = 0; i < GameManager.Instance.Cities.Length; i++)
        {
            if (GameManager.Instance.Cities[i].CityState != CityStates.DIRTY)
            {
                cleanCount++;
            }
        }

        if (cleanCount == GameManager.Instance.Cities.Length && GameManager.Instance.RoundStarted)
        {
            GameManager.Instance.EndRound();
            return;
        }

        Health = (float)cleanCount / (float)GameManager.Instance.Cities.Length;
        landMaterial.SetFloat("_Health", Health);
        RefreshTrees();
        atmosphereLight.color = Color.Lerp(dirtyColor, cleanColor, Health);
        waterMeshRenderer.material.color = Color.Lerp(waterDirtyColor, waterCleanColor, Health);
        Camera.main.backgroundColor = Color.Lerp(backgroundDirtyColor, backgroundCleanColor, Health);

        if (cloudParticleSystem)
        {
            Color newColor = Color.Lerp(cloudsDirtyColor, cloudsCleanColor, Health);
            cloudParticleSystem.startColor = newColor;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cloudParticleSystem.particleCount];

            int num = cloudParticleSystem.GetParticles(particles);

            for (int i = 0; i < num; i++)
            {
                particles[i].color = newColor;
            }
            // re-assign modified array
            cloudParticleSystem.SetParticles(particles, num);

            //////////////////////////////////

//            cloudParticleSystem.Clear();
//            cloudParticleSystem.Emit(1);
        }
    }

    private void InitializeTrees()
    {
        healthyTreesAtStart = Mathf.FloorToInt(startTrees * trees.Length);
        for (int i = healthyTreesAtStart; i < trees.Length; i++)
        {
            trees[i].SetUngrown();
        }
    }

    private void RefreshTrees()
    {
        int newHealthyTreeCount = healthyTreesAtStart +  Mathf.FloorToInt(Health * (trees.Length - healthyTreesAtStart));
        
        for (int i = 0; i < trees.Length; i++)
        {
            if (i < newHealthyTreeCount)
                trees[i].Grow();
            else
                trees[i].SetUngrown();
        }
    }

    private void HandleSpin()
    {
        momentum *= inertia;

        currentSpin = Mathf.Lerp(currentSpin, normalSpin, inertia) + momentum;


        transform.Rotate(Vector3.up, currentSpin * Time.deltaTime);
    }
}
