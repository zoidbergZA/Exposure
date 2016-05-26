using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
    public MeshRenderer scannableMesh;

    [SerializeField] private Light atmosphereLight;
    [SerializeField] private Color dirtyColor;
    [SerializeField] private Color cleanColor;

    private Chimney[] chimneys;

    public float Health { get; private set; }

    void Awake()
    {
        chimneys = FindObjectsOfType<Chimney>();

        RefreshHealth();
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
        Debug.Log("planet health: " + Health);
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
}
