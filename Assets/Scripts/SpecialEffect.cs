using UnityEngine;
using System.Collections;

public class SpecialEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleSystems;

    void Awake()
    {
        Reset();
    }

    public void Play()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        } 
    }

    public void Reset()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].isPlaying)
                particleSystems[i].Stop();
        }
    }
}
