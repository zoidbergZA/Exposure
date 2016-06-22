﻿using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
    public MeshRenderer scannableMesh;

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
    [SerializeField] private float normalSpin;

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
        RefreshHealth();
    }

    void Update()
    {
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
            if (!GameManager.Instance.Cities[i].IsDirty)
            {
                cleanCount++;
            }
        }

        Health = (float)cleanCount / (float)GameManager.Instance.Cities.Length;
        RefreshTrees();
        atmosphereLight.color = Color.Lerp(dirtyColor, cleanColor, Health);
        waterMeshRenderer.material.color = Color.Lerp(waterDirtyColor, waterCleanColor, Health);
        Camera.main.backgroundColor = Color.Lerp(backgroundDirtyColor, backgroundCleanColor, Health);

        if (cloudParticleSystem)
        {
            cloudParticleSystem.startColor = Color.Lerp(cloudsDirtyColor, cloudsCleanColor, Health);

            cloudParticleSystem.Clear();
            cloudParticleSystem.Emit(1);
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
        
        for (int i = 0; i < newHealthyTreeCount; i++)
        {
            trees[i].Grow();
        }
    }

    private void HandleSpin()
    {
        momentum *= inertia;

        currentSpin = Mathf.Lerp(currentSpin, normalSpin, inertia) + momentum;


        transform.Rotate(Vector3.up, currentSpin * Time.deltaTime);
    }
}
