using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light target;
    public float intensityBase = 2f;
    public float intensityAmplitude = 0.5f;
    public float speed = 6f;
    public float rangeBase = 6f;
    public float rangeAmplitude = 1f;
    float seed;

    void Awake(){ seed = Random.value * 100f; }

    void Update()
    {
        if (!target) return;
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        target.intensity = intensityBase + (n - 0.5f) * 2f * intensityAmplitude;
        target.range     = rangeBase     + (n - 0.5f) * 2f * rangeAmplitude;
    }
}