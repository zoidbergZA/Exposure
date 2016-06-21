using UnityEngine;
using System.Collections;

public class Volcano : MonoBehaviour
{
    [SerializeField] private float cooldownMin = 5f;
    [SerializeField] private float cooldownMax = 15f;
    [SerializeField] private ParticleSystem[] particleSystems;

    void Start()
    {
        StartCoroutine(InitErruption());
    }

    private IEnumerator InitErruption()
    {
        yield return new WaitForSeconds(Random.Range(cooldownMin, cooldownMax));

        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
    }
}
