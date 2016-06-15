using UnityEngine;
using System.Collections;

public class Shaker : MonoBehaviour 
{
    public float strength = 2f;
    private float shakeMagnitude = 1.0F;
    public float speed = 2f;
    public float decay = 0.925f;

    public bool IsShaking { get; private set; }

    private Transform other = null;

    void Start()
    {
//        Shake();
    }

    private void Update()
    {
        if (!other) ShakeGO();
        else ShakeGO(other);
    }

    private void ShakeGO(Transform other = null)
    {
        if (!IsShaking)
            return;

        //        float displacement = shakeMagnitude * (Mathf.PerlinNoise(Time.time*speed, 0.0F) - 0.5f);

        Vector3 displacement = new Vector3(
                shakeMagnitude * (Mathf.PerlinNoise(Time.time * speed, 0.0F) - 0.5f),
                shakeMagnitude * (Mathf.PerlinNoise(Time.time * speed, 40.0F) - 0.5f),
                shakeMagnitude * (Mathf.PerlinNoise(Time.time * speed, 130.0F) - 0.5f)
            );

        //        Vector3 pos = new Vector3();
        //        pos.y = displacement.x;
        //        pos.x = displacement.y;
        //        pos.z = displacement.z;
        if (!other) transform.localPosition = displacement;
        else other.transform.localPosition = displacement;

        shakeMagnitude *= decay;

        if (shakeMagnitude < 0.02f)
        {
            IsShaking = false;
            //            Shake(); //loop
        }
    }

    public void Shake()
    {
        IsShaking = true;
        shakeMagnitude = strength;
    }

    public void Shake(Transform other)
    {
        this.other = other;
        IsShaking = true;
        shakeMagnitude = strength;
    }
}
