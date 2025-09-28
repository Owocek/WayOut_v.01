using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0,24)] public float timeOfDay = 12f;
    public float dayLengthMinutes = 10f;
    public Transform sun, moon;
    public Light sunLight, moonLight;
    public AnimationCurve sunIntensity = AnimationCurve.Linear(0,0,12,1);
    public AnimationCurve moonIntensity = AnimationCurve.Linear(0,1,12,0);
    public Gradient ambientColor;

    void Update()
    {
        float dayProgressPerSec = 24f / (dayLengthMinutes * 60f);
        timeOfDay = (timeOfDay + dayProgressPerSec * Time.deltaTime) % 24f;
        float t = timeOfDay / 24f;

        float sunAngle = (t * 360f) - 90f;
        if (sun)  sun.localRotation  = Quaternion.Euler(sunAngle, 0f, 0f);
        if (moon) moon.localRotation = Quaternion.Euler(sunAngle + 180f, 0f, 0f);

        if (sunLight)  sunLight.intensity  = sunIntensity.Evaluate(timeOfDay);
        if (moonLight) moonLight.intensity = moonIntensity.Evaluate(timeOfDay);

        if (ambientColor.colorKeys.Length > 0)
            RenderSettings.ambientLight = ambientColor.Evaluate(t);
    }
}