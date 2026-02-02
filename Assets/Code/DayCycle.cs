using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class DayCycle : MonoBehaviour
{
    [Header("Day/Night Light Intensity")]
    [SerializeField] private Light2D targetLight;
    [SerializeField, Min(0f)] private float transitionDuration = 30f;
    [SerializeField, Min(0f)] private float dayCycle = 0f;
    [SerializeField, Min(0f)] private float minIntensity = 0.2f;
    [SerializeField, Min(0f)] private float maxIntensity = 1.0f;
    [SerializeField] private AnimationCurve intensityOverDay = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Day/Night Light Color")]
    // 357AA4 (min), A46735 (mid), FFFFFF (max)
    [SerializeField] private Color nightColor = new Color(53f / 255f, 122f / 255f, 164f / 255f, 1f);
    [SerializeField] private Color sunRiseColor = new Color(164f / 255f, 103f / 255f, 53f / 255f, 1f);
    [SerializeField] private Color dayColor = Color.white;

    private float t01;
    private bool goingUp = true;
    private float holdRemainingSeconds;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (targetLight == null) return;
        if (transitionDuration <= 0f) return;

        float dt = Time.deltaTime;
        if (holdRemainingSeconds > 0f)
        {
            holdRemainingSeconds -= dt;
            if (holdRemainingSeconds < 0f) holdRemainingSeconds = 0f;
        }
        else
        {
            float oneWaySeconds = Mathf.Max(0.0001f, transitionDuration * 0.5f);
            float step = dt / oneWaySeconds;
            t01 += goingUp ? step : -step;

            if (t01 >= 1f)
            {
                t01 = 1f;
                goingUp = false;
                holdRemainingSeconds = dayCycle;
            }
            else if (t01 <= 0f)
            {
                t01 = 0f;
                goingUp = true;
                holdRemainingSeconds = dayCycle;
            }
        }

        float curveT = Mathf.Clamp01(intensityOverDay.Evaluate(t01));
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, curveT);

        // Color transitions: min -> mid -> max (and back), based on curveT
        if (curveT <= 0.5f)
        {
            float k = Mathf.InverseLerp(0f, 0.5f, curveT);
            targetLight.color = Color.Lerp(nightColor, sunRiseColor, k);
        }
        else
        {
            float k = Mathf.InverseLerp(0.5f, 1f, curveT);
            targetLight.color = Color.Lerp(sunRiseColor, dayColor, k);
        }
    }
}
