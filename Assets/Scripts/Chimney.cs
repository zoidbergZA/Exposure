using UnityEngine;
using System.Collections;

public class Chimney : MonoBehaviour
{
    public enum ChimneyStates
    {
        Working,
        Unused,
        Exploded
    }

    [SerializeField] private ParticleSystem smokeSystem;

    public ChimneyStates ChimneyState { get; private set; }

    void Awake()
    {
        ChimneyState = ChimneyStates.Working;
    }

    public void DisableChimney()
    {
        if (ChimneyState != ChimneyStates.Working)
            return;

        ChimneyState = ChimneyStates.Unused;
        smokeSystem.enableEmission = false;
    }
}
