using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class Flasher : MonoBehaviour
{
    private Light light;
    private float brightness;

    void Awake()
    {
        light = GetComponent<Light>();
        brightness = light.intensity;
    }

    void Start()
    {
        LeanTween.value(gameObject, updateBrightnessCallback, light.intensity, 0f, 1.7f).setEase(LeanTweenType.easeOutExpo).setLoopClamp();
    }

    void Update()
    {
        light.intensity = brightness;
    }

    void updateBrightnessCallback(float val)
    {
        brightness = val;
    }
}
