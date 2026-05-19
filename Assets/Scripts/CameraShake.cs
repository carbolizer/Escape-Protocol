using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private float shakeTimer;
    private float shakeIntensity;
    private Vector3 originalLocalPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        originalLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f)
        {
            transform.localPosition = originalLocalPosition;
            return;
        }

        shakeTimer -= Time.unscaledDeltaTime;
        float dampen = Mathf.Clamp01(shakeTimer / 0.25f);
        Vector2 offset = Random.insideUnitCircle * shakeIntensity * dampen;
        transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
    }

    public void Shake(float intensity, float duration)
    {
        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }
}
