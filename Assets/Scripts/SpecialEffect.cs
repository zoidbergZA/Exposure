using UnityEngine;
using System.Collections;

public class SpecialEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleSystems;

    void Awake()
    {
        Reset();
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].transform.position = position;
            particleSystems[i].transform.rotation = rotation;
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
