using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class Erruptor : MonoBehaviour
{
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float duration = 3f;

    private ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.enableEmission = false;
    }

    void Start()
    {
        StartCoroutine(BuildUp());
    }

    private IEnumerator BuildUp()
    {
        yield return new WaitForSeconds(cooldown);

        StartCoroutine(Errupt());
    }

    private IEnumerator Errupt()
    {
        particleSystem.enableEmission = true;

        yield return new WaitForSeconds(duration);

        particleSystem.enableEmission = false;
        StartCoroutine(BuildUp());
    } 
}
